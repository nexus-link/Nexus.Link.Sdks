using System.Transactions;
using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

/// <summary>
/// HelperClasses around transactions
/// </summary>
public static class TransactionHelper
{
    /// <summary>
    /// Get <see cref="System.Transactions.Transaction.Current"/>, but catch exceptions.
    /// Return null if current is null or if there is an exception.
    /// </summary>
    public static System.Transactions.Transaction GetCurrentNotCompleteTransaction()
    {
        try
        {
            return System.Transactions.Transaction.Current;
        }
        catch (Exception)
        {
            // Complete transactions throws exception
            return null;
        }
    }

    /// <summary>
    /// Create a scope with our standard options
    /// </summary>
    /// <returns></returns>
    public static TransactionScope CreateStandardScope()
    {

        var options = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted
        };
        return new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
    }

    /// <summary>
    /// Create a suppressed scope (ignore outer scope), with our standard options
    /// </summary>
    /// <returns></returns>
    public static TransactionScope CreateSuppressedScope()
    {

        var options = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted
        };
        return new TransactionScope(TransactionScopeOption.Suppress, options, TransactionScopeAsyncFlowOption.Enabled);
    }
}