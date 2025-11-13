using NCalc;

public class Predicate
{
    public Expression _NCalcExpression;

    public bool _HasQuantifiers; 
    public Predicate(Expression ncalcExpression, bool hasQuantifiers)
    {
        _NCalcExpression = ncalcExpression;
        _HasQuantifiers = hasQuantifiers;
    }
}