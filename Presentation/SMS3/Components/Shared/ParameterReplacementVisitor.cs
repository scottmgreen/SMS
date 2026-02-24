namespace SMS3.Components.Shared;

/// <summary>
/// Helper class for replacing parameters in LINQ expressions
/// </summary>
public class ParameterReplacementVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly Expression _newExpression;

    public ParameterReplacementVisitor(ParameterExpression oldParameter, Expression newExpression)
    {
        _oldParameter = oldParameter;
        _newExpression = newExpression;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newExpression : base.VisitParameter(node);
    }
}