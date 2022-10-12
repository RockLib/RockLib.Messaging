---
sidebar_position: 13
---

# Sending messages transactionally

The RockLib.Messaging library defines an interface for sending messages tranactionally through the `ITransactionalSender` interface. Sending messages transactionally in this case means that one or more messages, all destined for the same sender, are added to a transaction, then the transaction is committed. The `ITransactionalSender` interface inherits from `ISender`, so it has the same functionality in addition to its `BeginTransaction` method.

```csharp
ITransactionalSender transactionalSender = // TODO: Instantiate.

ISenderTransaction transaction = transactionalSender.BeginTransaction();

transaction.Add(new SenderMessage("First message"));
transaction.Add(new SenderMessage("Second message"));
transaction.Add(new SenderMessage("Third message"));

bool rollback = false;

// Do stuff that may or may not succeed. If it does not, set 'rollback' to true.

if (!rollback)
    transaction.Commit();
else
    transaction.Rollback();
```
