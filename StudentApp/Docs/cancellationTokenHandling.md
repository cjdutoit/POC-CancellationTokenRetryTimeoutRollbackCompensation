# Cancellation Token Handling

## 1. Overview

### 1.1 What is a CancellationToken?

A `CancellationToken` is a signal that says **"please stop what you are doing"**. It does not
stop your code automatically — it is just a flag that your code can check. You decide when to
look at it and what to do when it is set.

Think of it like a referee holding up a red card during a match. The referee does not physically
stop the game — the players have to notice the card and choose to stop.

---

### 1.2 What This Example Demonstrates

This example shows a two-layer system that handles cancellation at the right place for the
right reasons:

**Layer 1 — Foundation Services** (`StudentService`, `LibraryService`)

These services sit directly above the database. Their job is narrow:

- If the database timed out on its own (an infrastructure problem), wrap it in a
  `TimeoutStudentException` so the caller gets a meaningful error.
- If the cancellation was intentional (someone called `ThrowIfCancellationRequested()`),
  re-throw it as-is without wrapping it, so the caller can still see it clearly.

The foundation service does **not** decide what to do about the cancellation. It simply
passes the signal up cleanly.

**Layer 2 — Orchestration Service** (`StudentOrchestrationService`)

This service coordinates multiple foundation service calls in a specific order (A to B to C).
Because it knows the **business context** — what has already been saved and what has not — it
is the right place to decide what a cancellation actually means:

| Step | What has happened | Decision |
|------|------------------|----------|
| **A** - Add Student | Nothing saved yet | Cancel safely, no consequences |
| **B** - Add Library Card | Student is in the database | Warn that the student exists but has no library card, then cancel |
| **C** - Record Audit | Student and library card both exist | Too late - shield this step and let it finish |

The key insight is this separation of concerns:

> **Foundation services observe the cancellation token.**
> **The orchestration service decides what to do about it.**

---

### 1.3 Why This Pattern Exists

A common mistake is to either:

1. **Ignore cancellation entirely** - the app keeps running wasted work after the user has
   already given up or the request has timed out.
2. **Cancel blindly everywhere** - the app stops mid-way through a multi-step process and
   leaves the database in an inconsistent state (a student exists with no library card, and
   nobody logged a warning about it).

This pattern avoids both problems by making cancellation **intentional and context-aware**
at the orchestration level, while keeping the foundation services simple and reusable.

---

### 1.4 How Foundation Services Classify OperationCanceledException

The foundation services distinguish between two types of `OperationCanceledException`:

| Source | IsCancellationRequested | Result |
|--------|------------------------|--------|
| Infrastructure timeout (EF Core, HttpClient, etc.) | `false` | Caught and wrapped as `TimeoutStudentException` then `StudentDependencyException` |
| `ThrowIfCancellationRequested()` or user cancellation | `true` | Re-thrown cleanly and bubbles up to orchestration |

The value of `IsCancellationRequested` is the key that separates these two cases, and the
`when` filter on the first catch block uses it to decide which path to take.

---

## 2. Foundation Service - StudentService

### 2.1 The Public Method

The public-facing method passes a delegate into `TryCatch`. The first line of that delegate
calls `ThrowIfCancellationRequested()`. This is the earliest possible point to honour the
cancellation signal, before any validation or database work begins.

```csharp
public ValueTask<Student> AddStudentAsync(
    Student student,
    CancellationToken cancellationToken) =>
    TryCatch(async () =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateStudentOnAdd(student);

        return await this.storageBroker.InsertStudentAsync(student, cancellationToken);
    });
```

### 2.2 The TryCatch Wrapper

The `TryCatch` method wraps the delegate in a try/catch. It has two catch blocks for
`OperationCanceledException` — one for infrastructure timeouts and one for intentional
cancellations. These must be in this order because the `when` filter on the first block
only matches timeouts. The second block handles everything else.

```csharp
private async ValueTask<Student> TryCatch(ReturningStudentFunction returningStudentFunction)
{
    try
    {
        return await returningStudentFunction();
    }
    catch (OperationCanceledException operationCanceledException)
        when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
    {
        // This block only matches when IsCancellationRequested is FALSE.
        // That means our code never requested cancellation — the infrastructure
        // (EF Core or HttpClient) raised this exception on its own due to a timeout.
        // We wrap it in a meaningful exception type and throw.
        var timeoutException =
            new TimeoutException(
                "The dependency operation timed out.");

        var timeoutStudentException =
            new TimeoutStudentException(
                message: "Failed student timeout error occurred, contact support.",
                innerException: timeoutException,
                data: timeoutException.Data);

        throw CreateAndLogDependencyException(timeoutStudentException);
    }
    catch (OperationCanceledException)
    {
        // This block matches when IsCancellationRequested is TRUE.
        // ThrowIfCancellationRequested() always produces an exception where
        // IsCancellationRequested is true, so it always lands here.
        // We do not wrap it — we re-throw it exactly as-is so the orchestration
        // service above us can receive the raw signal and decide what to do with it.
        throw;
    }
}
```

### 2.3 How ThrowIfCancellationRequested Routes Through the Catch Blocks

When `ThrowIfCancellationRequested()` fires inside the delegate, the exception it produces
always has `IsCancellationRequested = true`. Here is the exact routing:

```
cancellationToken.ThrowIfCancellationRequested() fires
        |
        v
OperationCanceledException is thrown
  .CancellationToken.IsCancellationRequested = true  (always, by definition)
        |
        v
First catch block evaluates its when filter:
  when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
  --> false is false? No. Filter does NOT match. This catch block is skipped.
        |
        v
Second catch block:
  catch (OperationCanceledException)  --> matches
  { throw; }  --> re-thrown without any wrapping
        |
        v
Exception propagates out of TryCatch
        |
        v
Received by the orchestration service
```

This is why the order of the two catch blocks matters. The `when` filter on the first block
acts as a gate — if the token was cancelled by our code, the first block steps aside and the
second block handles it.

---

## 3. Foundation Service - LibraryService

### 3.1 The Public Method

The `LibraryService` follows the exact same pattern. The public method checks the token first,
and the `TryCatch` wrapper uses the same two catch blocks to route timeouts and intentional
cancellations separately.

```csharp
public ValueTask<LibraryCard> AddLibraryCardApplicationAsync(
    Student student,
    CancellationToken cancellationToken) =>
    TryCatch(async () =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateLibraryCardOnAdd(student);

        return await this.storageBroker.InsertLibraryCardAsync(student, cancellationToken);
    });
```

### 3.2 The TryCatch Wrapper

```csharp
private async ValueTask<LibraryCard> TryCatch(ReturningLibraryCardFunction returningLibraryCardFunction)
{
    try
    {
        return await returningLibraryCardFunction();
    }
    catch (OperationCanceledException operationCanceledException)
        when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
    {
        var timeoutException =
            new TimeoutException(
                "The dependency operation timed out.");

        var timeoutLibraryCardException =
            new TimeoutLibraryCardException(
                message: "Failed library card timeout error occurred, contact support.",
                innerException: timeoutException,
                data: timeoutException.Data);

        throw CreateAndLogDependencyException(timeoutLibraryCardException);
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    // ... other catches
}
```

The routing through the catch blocks is identical to section 2.3.

---

## 4. Orchestration Service - StudentOrchestrationService

### 4.1 Structure

The orchestration service has the same two-part structure as the foundation services — a
public method that passes a delegate into `TryCatch`, and a `TryCatch` wrapper that handles
exceptions. The difference is what it catches. Foundation services catch raw infrastructure
exceptions. The orchestration service catches the already-classified exceptions that the
foundation services produce.

### 4.2 The Public Method

```csharp
public ValueTask<Student> OrchestrateStudentOnboardingAsync(
    Student student,
    CancellationToken cancellationToken) =>
    TryCatch(async () =>
    {
        // Check the token before doing anything at all.
        // If already cancelled, OperationCanceledException bubbles through
        // the TryCatch re-throw and exits immediately. Nothing is saved.
        cancellationToken.ThrowIfCancellationRequested();
        ValidateStudentOnOrchestrate(student);

        // Variables declared here so the inner catch block can see them
        // and log a meaningful warning about what was already saved.
        Student createdStudent = null;
        LibraryCard createdLibraryCard = null;

        try
        {
            // ── Step A ────────────────────────────────────────────────────────
            // Nothing has been saved yet. If the token is cancelled at any point
            // during AddStudentAsync — whether at its own ThrowIfCancellationRequested()
            // or mid-await in the storage broker — the foundation TryCatch re-throws
            // OperationCanceledException. That exception propagates out here, skips
            // the rest of the try block, and is caught by the inner catch below.
            createdStudent =
                await this.studentService.AddStudentAsync(student, cancellationToken);

            // ── Step B ────────────────────────────────────────────────────────
            // The student is now in the database. Before calling LibraryService,
            // we explicitly check the token. This is the only place where we are
            // in normal code flow AND we know the student was saved. If we skip
            // this check and the token is already cancelled, LibraryService's own
            // ThrowIfCancellationRequested() would still fire — but by then we are
            // inside another service call and cannot log a warning with context.
            if (cancellationToken.IsCancellationRequested)
            {
                // Log before throwing — once we throw, this context is lost.
                this.loggingBroker.LogWarning(
                    $"Student '{createdStudent.Id}' was created but cancellation was " +
                    $"requested. Library card application will NOT be submitted.");

                cancellationToken.ThrowIfCancellationRequested();
            }

            createdLibraryCard =
                await this.libraryService.AddLibraryCardApplicationAsync(
                    createdStudent,
                    cancellationToken);

            // ── Step C ────────────────────────────────────────────────────────
            // Both the student and library card are in the database. It is too
            // late to cancel. We pass CancellationToken.None so that the audit
            // service's ThrowIfCancellationRequested() checks a token that is
            // never cancelled. This step always runs to completion.
            await this.auditService.RecordStudentEnrollmentAsync(
                createdStudent,
                createdLibraryCard,
                CancellationToken.None);

            return createdStudent;
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException reached this inner catch, which means
            // cancellation fired during Step A or Step B. We now know what was
            // partially saved (createdStudent and createdLibraryCard may be null
            // or set, depending on which step was interrupted).
            //
            // This is the point where you decide your recovery strategy.
            // See section 5 - Rollback vs Compensation.
            await HandleCancellationAsync(createdStudent, createdLibraryCard);

            // Re-throw so the outer TryCatch wrapper (and ultimately the caller)
            // still receives the OperationCanceledException. We do not swallow it.
            throw;
        }
    });
```

### 4.3 Why There Are Two Catch Levels

The inner `try/catch` inside the delegate exists because the outer `TryCatch` wrapper only
sees exceptions after the entire delegate has thrown. By the time the outer `TryCatch` gets
the `OperationCanceledException`, the delegate has already exited and we no longer have
access to `createdStudent` or `createdLibraryCard`.

The inner catch gives us a window to act on partial state — log what was saved, trigger
compensation — before re-throwing to let the outer wrapper propagate the signal cleanly.

```
OperationCanceledException thrown inside Step A or Step B
        |
        v
Inner catch (OperationCanceledException) inside the delegate
  - Sees createdStudent (may be set or null)
  - Sees createdLibraryCard (may be set or null)
  - Calls HandleCancellationAsync to log or compensate
  - Re-throws
        |
        v
Exception exits the delegate
        |
        v
Outer TryCatch: catch (OperationCanceledException) { throw; }
  - No wrapping — re-throws as-is
        |
        v
Caller receives raw OperationCanceledException
```

### 4.4 The Outer TryCatch Wrapper

```csharp
private async ValueTask<Student> TryCatch(ReturningStudentFunction returningStudentFunction)
{
    try
    {
        return await returningStudentFunction();
    }
    catch (OperationCanceledException)
    {
        // The inner catch already handled logging and compensation.
        // Here we simply re-throw so the caller receives the signal.
        // We must catch it explicitly here — without this block it would
        // fall into the Exception catch below and get buried inside a
        // StudentOrchestrationServiceException, hiding the cancellation.
        throw;
    }
    catch (StudentDependencyException studentDependencyException)
    {
        var studentOrchestrationDependencyException =
            new StudentOrchestrationDependencyException(
                message: "Student orchestration dependency error occurred, contact support.",
                innerException: studentDependencyException);

        throw CreateAndLogDependencyException(studentOrchestrationDependencyException);
    }
    catch (StudentServiceException studentServiceException)
    {
        var studentOrchestrationServiceException =
            new StudentOrchestrationServiceException(
                message: "Student orchestration service error occurred, contact support.",
                innerException: studentServiceException);

        throw CreateAndLogServiceException(studentOrchestrationServiceException);
    }
    catch (Exception exception)
    {
        var studentOrchestrationServiceException =
            new StudentOrchestrationServiceException(
                message: "Unexpected orchestration error occurred, contact support.",
                innerException: exception);

        throw CreateAndLogServiceException(studentOrchestrationServiceException);
    }
}
```

---

## 5. Rollback vs Compensation

### 5.1 Option 1 - Rollback

Rollback means attempting to delete whatever was partially saved, as if the operation never
happened. This is appropriate in simple systems where nothing outside your database has
observed the change yet.

```csharp
private async ValueTask HandleCancellationAsync(
    Student student,
    LibraryCard libraryCard)
{
    if (libraryCard is not null)
    {
        await this.libraryService.RemoveLibraryCardAsync(
            libraryCard.Id,
            CancellationToken.None);
    }

    if (student is not null)
    {
        await this.studentService.RemoveStudentAsync(
            student.Id,
            CancellationToken.None);
    }
}
```

Note that both calls use `CancellationToken.None`. The original token is already cancelled —
passing it to the rollback calls would cause them to be cancelled immediately too, leaving
the database in the same inconsistent state you were trying to fix.

### 5.2 Option 2 - Compensation

**Rollback is not always safe.** If other systems have already observed the change — for
example, an event was published when the student was created, and a downstream system has
already processed it — then deleting the student record creates a different kind of
inconsistency.

A safer pattern in event-driven or distributed systems is compensation: instead of deleting
the record, mark it as incomplete and let a background process handle recovery.

```csharp
private async ValueTask HandleCancellationAsync(
    Student student,
    LibraryCard libraryCard)
{
    if (student is not null)
    {
        student.EnrollmentStatus = EnrollmentStatus.Incomplete;

        await this.studentService.ModifyStudentAsync(
            student,
            CancellationToken.None);

        this.loggingBroker.LogWarning(
            $"Student '{student.Id}' marked as incomplete due to cancellation. " +
            $"Library card was {(libraryCard is null ? "not created" : "created")}. " +
            $"A background process should review this record.");
    }
}
```

### 5.3 Which to Choose

| Situation | Use |
|-----------|-----|
| Simple database-only system, no events published yet | Rollback |
| Event-driven system — events published on student creation | Compensation |
| Third-party APIs called during the process | Compensation |
| Distributed transactions or sagas | Compensation |

**Rollback says "pretend it never happened". Compensation says "acknowledge what happened
and reverse it intentionally".** In modern distributed systems, compensation is almost
always the safer choice.

---

## 6. Cancellation Bubble-Up: Step by Step

The following traces show exactly what happens when cancellation fires at each point in the
orchestration flow.

### 6.1 Before Step A — Token Already Cancelled on Entry

```
OrchestrateStudentOnboardingAsync called with a cancelled token
        |
        v
cancellationToken.ThrowIfCancellationRequested() at top of delegate fires
        |
        v
OperationCanceledException (IsCancellationRequested = true)
        |
        v
No inner catch is active yet (we are before the inner try block)
        |
        v
Exception exits the delegate
        |
        v
Outer TryCatch: catch (OperationCanceledException) { throw; }
        |
        v
Caller receives OperationCanceledException.
Nothing was saved. No compensation needed.
```

### 6.2 During Step A — Cancellation Fires Inside AddStudentAsync

```
createdStudent = await this.studentService.AddStudentAsync(student, cancellationToken)
        |
        |  Inside AddStudentAsync, ThrowIfCancellationRequested() fires
        |  (or cancellation fires mid-await in the storage broker with IsCancellationRequested = true)
        |
        v
Foundation TryCatch (see section 2.2):
  First catch: when (IsCancellationRequested is false) --> does not match, skipped
  Second catch: catch (OperationCanceledException) { throw; } --> matches, re-thrown
        |
        v
OperationCanceledException propagates out of AddStudentAsync
        |
        v
Inner catch (OperationCanceledException) inside the orchestration delegate (see section 4.3)
  createdStudent is null — AddStudentAsync did not complete
  createdLibraryCard is null
  HandleCancellationAsync called — nothing to roll back or compensate
  Re-throws
        |
        v
Outer TryCatch: catch (OperationCanceledException) { throw; } (see section 4.4)
        |
        v
Caller receives OperationCanceledException.
Nothing was saved.
```

### 6.3 Between Step A and Step B — Token Cancelled After AddStudentAsync Returned

```
AddStudentAsync completed — student is now in the database
createdStudent is set

Token becomes cancelled before we reach the if check (or was already cancelled)

if (cancellationToken.IsCancellationRequested)  --> true
        |
        v
Warning is logged:
  "Student '{id}' was created but cancellation was requested.
   Library card application will NOT be submitted."
        |
        v
cancellationToken.ThrowIfCancellationRequested() fires
        |
        v
Inner catch (OperationCanceledException) inside the orchestration delegate (see section 4.3)
  createdStudent is set — student IS in the database
  createdLibraryCard is null — library card was NOT created
  HandleCancellationAsync called with the student record
  Compensation or rollback applied (see section 5)
  Re-throws
        |
        v
Outer TryCatch: catch (OperationCanceledException) { throw; } (see section 4.4)
        |
        v
Caller receives OperationCanceledException.
```

This is the only scenario where the warning is logged with context, because it is the only
point where we are in normal code flow, the student is saved, and we have not yet entered
another service call.

### 6.4 During Step B — Cancellation Fires Inside AddLibraryCardApplicationAsync

```
The if check passed (token was not yet cancelled)

createdLibraryCard = await this.libraryService.AddLibraryCardApplicationAsync(...)
        |
        |  Inside AddLibraryCardApplicationAsync, ThrowIfCancellationRequested() fires
        |  Foundation TryCatch re-throws OperationCanceledException (see section 3.2)
        |
        v
OperationCanceledException propagates out of AddLibraryCardApplicationAsync
        |
        v
Inner catch (OperationCanceledException) inside the orchestration delegate (see section 4.3)
  createdStudent is set — student IS in the database
  createdLibraryCard is null — library card was NOT created (call did not complete)
  HandleCancellationAsync called — can log that the student exists without a library card
  Compensation or rollback applied (see section 5)
  Re-throws
        |
        v
Outer TryCatch: catch (OperationCanceledException) { throw; } (see section 4.4)
        |
        v
Caller receives OperationCanceledException.
```

No warning is logged before this path. The if check before Step B only catches the case
where the token was already cancelled before we entered the call. Once we are inside
`AddLibraryCardApplicationAsync`, any cancellation follows this direct bubble-up path.
The inner catch still has access to `createdStudent` and can compensate accordingly.

### 6.5 Between Step B and Step C — Token Cancelled After AddLibraryCardApplicationAsync Returned

```
AddLibraryCardApplicationAsync completed — library card is now in the database
createdLibraryCard is set

Token is now cancelled — but Step C receives CancellationToken.None

await this.auditService.RecordStudentEnrollmentAsync(
    createdStudent,
    createdLibraryCard,
    CancellationToken.None)
        |
        |  Inside RecordStudentEnrollmentAsync, ThrowIfCancellationRequested()
        |  checks CancellationToken.None.IsCancellationRequested
        |  CancellationToken.None is never cancelled --> always false --> no-op
        |
        v
Step C runs to completion normally.
The orchestration returns the created student.
No exception is thrown. The original cancelled token is irrelevant from this point.
```

### 6.6 During or After Step C

Step C always completes because it receives `CancellationToken.None`. There is no scenario
where cancellation interrupts Step C.

---

## 7. Step Behaviour Summary

| Step | Token Passed | Cancellation Behaviour |
|------|-------------|----------------------|
| **Entry** | `cancellationToken` | ThrowIfCancellationRequested fires, nothing saved, exits immediately (see 6.1) |
| **A** - Add Student | `cancellationToken` | Foundation re-throws, inner catch handles, nothing saved (see 6.2) |
| **Between A and B** | checked via `IsCancellationRequested` | Warning logged with context, inner catch compensates (see 6.3) |
| **B** - Add Library Card | `cancellationToken` | Foundation re-throws, inner catch handles, student exists without library card (see 6.4) |
| **Between B and C** | `CancellationToken.None` passed to C | No-op, Step C completes, data is consistent (see 6.5) |
| **C** - Record Audit | `CancellationToken.None` | Always completes, cancellation has no effect (see 6.6) |


---

## 8. Design Decisions: Retry and Compensation

### 8.1 The Problem with the Inline Inner Try/Catch

As shown in section 4.2, the orchestration method currently contains a raw `try/catch`
block inside the delegate. This works correctly, but it mixes two concerns inside one
method: the business logic of steps A, B and C, and the cancellation-compensation
infrastructure. As the number of orchestration methods grows, this pattern would be
repeated in every one of them.

A cleaner approach is to extract the inner `try/catch` into a dedicated
`TryCatchWithCompensationAsync` overload that accepts a second delegate for compensation â€”
keeping the business logic method free of exception handling infrastructure entirely.

---

### 8.2 Retry at the Foundation Layer

The foundation `TryCatch` shown in sections 2.2 and 3.2 handles timeout by delegating to
a `TryCatchWithRetry` helper â€” giving the operation one additional attempt before
wrapping and throwing. Because timeout classification already happens inside `TryCatch`,
retry belongs there too â€” not in the orchestration service, which has no business knowing
whether a storage call was attempted once or twice.

```csharp
catch (OperationCanceledException operationCanceledException)
	when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
{
	var timeoutException = new TimeoutException("The dependency operation timed out.");

	var timeoutStudentException =
		new TimeoutStudentException(
			message: "Failed student timeout error occurred, contact support.",
			innerException: timeoutException,
			data: timeoutException.Data);

	// On timeout, retry the delegate once before giving up.
	return await TryCatchWithRetry(
		returningStudentFunction,
		async () => await CreateAndLogDependencyException(timeoutStudentException));
}
```

`TryCatchWithRetry` re-attempts the original delegate. If the retry also fails, it
calls the supplied factory to create and log the dependency exception and throws it.

```csharp
private async ValueTask<Student> TryCatchWithRetry(
	ReturningStudentFunction returningStudentFunction,
	Func<ValueTask<Xeption>> createAndLogExceptionAsync)
{
	try
	{
		return await returningStudentFunction();
	}
	catch
	{
		throw await createAndLogExceptionAsync();
	}
}
```

This mechanism is intentionally scoped to `TimeoutException` today but can be extended to
any other transient exception that warrants a retry â€” the pattern is the same: detect in
the appropriate catch block, delegate to `TryCatchWithRetry` with the relevant exception
factory. The orchestration layer is unaffected either way â€” it only ever receives a final,
non-transient `StudentDependencyException` once all attempts are exhausted.

The intentional-cancellation catch is unchanged â€” it still re-throws immediately, and the
retry path never applies to it.

---

### 8.3 The Pattern: TryCatchWithCompensationAsync

The public method keeps the standard `=>` expression body and the existing outer `TryCatch`
wrapper â€” exactly as in the foundation services. The inner overload is nested **inside** the
outer delegate, which is what allows the closure to work.

Two delegates are passed to `TryCatchWithCompensationAsync`:

1. The **primary delegate** â€” the business logic (steps A, B and C).
2. The **compensation delegate** â€” what to undo with partial state if cancellation fires.

Retry is not a concern here. Foundation services handle timeout retries transparently
(see section 8.2). By the time any exception reaches this overload, it has either already
been retried and exhausted at the foundation layer, or it was never a transient timeout to
begin with.

The closure connects the two delegates.
`createdStudent` and `createdLibraryCard` are declared inside the outer `TryCatch` delegate â€”
the primary delegate assigns to them as each step completes, and the compensation delegate
reads them when cancellation fires. The overload itself never needs to see these variables.

```csharp
private delegate ValueTask<Student> ReturningStudentFunction();
private delegate ValueTask CompensatingStudentFunction();
```

---

### 8.4 The Orchestration Method (Cleaned Up)

```csharp
public ValueTask<Student> OrchestrateStudentOnboardingAsync(
	Student student,
	CancellationToken cancellationToken) =>
	TryCatch(async () =>
	{
		// Declared inside the outer delegate so both inner lambdas can close over them.
		// The primary delegate assigns to them as each step completes.
		// The compensation delegate reads them if cancellation fires.
		Student createdStudent = null;
		LibraryCard createdLibraryCard = null;

		return await TryCatchWithCompensationAsync(
			returningStudentFunction: async () =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				ValidateStudentOnOrchestrate(student);

				// Assignment captured in the closure â€” compensation can see this.
				createdStudent =
					await this.studentService.AddStudentAsync(student, cancellationToken);

				if (cancellationToken.IsCancellationRequested)
				{
					this.loggingBroker.LogWarning(
						$"Student '{createdStudent.Id}' was created but cancellation was " +
						$"requested. Library card application will NOT be submitted.");

					cancellationToken.ThrowIfCancellationRequested();
				}

				// Assignment captured in the closure â€” compensation can see this.
				createdLibraryCard =
					await this.libraryService.AddLibraryCardApplicationAsync(
						createdStudent,
						cancellationToken);

				await this.auditService.RecordStudentEnrollmentAsync(
					createdStudent,
					createdLibraryCard,
					CancellationToken.None);

				return createdStudent;
			},
			compensatingStudentFunction: async () =>
			{
				// Both variables are read from the closure.
				// If cancellation fired before Step A completed, createdStudent is null.
				// If cancellation fired before Step B completed, createdLibraryCard is null.
				await HandleCancellationAsync(createdStudent, createdLibraryCard);
			});
	});
```

The public method remains an expression body with a single outer `TryCatch` â€” identical in
shape to every other orchestration method. The inner overload handles cancellation and
compensation. Timeout retries are invisible here â€” they were already exhausted or succeeded
inside the foundation services.

---

### 8.5 The TryCatchWithCompensationAsync Overload

```csharp
private async ValueTask<Student> TryCatchWithCompensationAsync(
	ReturningStudentFunction returningStudentFunction,
	CompensatingStudentFunction compensatingStudentFunction)
{
	try
	{
		return await returningStudentFunction();
	}
	catch (OperationCanceledException operationCanceledException)
		when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
	{
		// Timeout that escaped a foundation service â€” compensate, then wrap and throw.
		await compensatingStudentFunction();

		var timeoutException =
			new TimeoutException("The dependency operation timed out.");

		var timeoutStudentOrchestrationException =
			new TimeoutStudentOrchestrationException(
				message: "Student orchestration timeout error occurred, contact support.",
				innerException: timeoutException,
				data: timeoutException.Data);

		throw CreateAndLogDependencyException(timeoutStudentOrchestrationException);
	}
	catch (OperationCanceledException)
	{
		// Intentional cancellation â€” compensate, then re-throw so the outer
		// TryCatch wrapper still receives the signal.
		await compensatingStudentFunction();
		throw;
	}
	catch
	{
		// All other exceptions (dependency, service, unexpected) are already classified
		// and logged by the outer TryCatch wrapper â€” re-throw without modification.
		throw;
	}
}
```

The overload knows nothing about `createdStudent` or `createdLibraryCard`, and nothing
about retry. Compensation runs in exactly two situations: an unhandled timeout, and an
intentional cancellation. All other exception classification is left to the outer `TryCatch`
wrapper, which already handles every case â€” keeping the overload focused on a single
responsibility.

---

### 8.6 Responsibility by Layer

The key architectural principle is that each layer owns the exception-handling
responsibility appropriate to its execution concern:

- **Foundation services** deal with **retry**. They are closest to the storage dependency
  and are the right place to decide whether a transient failure (such as a timeout) is
  worth attempting again. The retry mechanism is currently scoped to `TimeoutException`
  but can be extended to any exception that warrants a retry without changing the layers
  above.

- **Processing services and above** deal with **compensation or rollback**. By the time
  control reaches these layers, the foundation service has already made its retry decision.
  What remains is the question of whether partial state written across multiple services
  needs to be undone â€” that is a business-logic concern, not a transient-failure concern.

| Layer | Responsibility |
|-------|---------------|
| Foundation | Timeout detection, retry, and final exception classification |
| Orchestration and above | Compensation or rollback of partial cross-service state |

---

### 8.7 When to Use Each Overload

| Overload | Use When |
|----------|---------|
| `TryCatch` | Single-step operation â€” foundation services always use this |
| `TryCatchWithCompensationAsync` | Multi-step writes where partial state needs compensation if cancellation or timeout fires |

The rule is: start with `TryCatch`. Add `TryCatchWithCompensationAsync` when the operation
has side effects across multiple services that could leave the database in a partial state.
Timeout retry is never a consideration at this layer â€” it is handled inside each
foundation service before the exception ever reaches the orchestration layer.

---

### 8.8 What This Achieves

| Concern | Where it lives |
|---------|---------------|
| Timeout detection and retry | Foundation service `TryCatch` + `TryCatchWithRetry` (sections 2.2, 3.2, 8.2) |
| Cancellation compensation | Orchestration `TryCatchWithCompensationAsync` (section 8.5) |
| Business logic | Orchestration primary delegate â€” no exception infrastructure |
| Shared partial state | Closure variables in the outer delegate |

| Before (section 4.2) | After (section 8.4) |
|---------------------|---------------------|
| Inline `try/catch` inside every orchestration method | Compensation handled once in `TryCatchWithCompensationAsync` |
| `createdStudent` and `createdLibraryCard` declared inside the inline try block | Declared in the outer delegate â€” both lambdas share them via closure |
| Mixed business logic and exception infrastructure | Business logic reads as a clean sequential flow |
| Public method required a block body to declare the variables | Public method keeps the standard `=>` expression body |
| Retry logic mixed into the orchestration layer | Retry is invisible to orchestration â€” handled inside each foundation service |