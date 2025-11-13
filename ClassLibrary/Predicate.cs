using NCalc;

public class Predicate
{
    public Expression _NCalcExpression;
    public bool _HasQuantifiers;

    public string VariableName;


    public Predicate(Expression ncalcExpression, bool hasQuantifiers, string variableName)
    {
        _NCalcExpression = ncalcExpression;
        _HasQuantifiers = hasQuantifiers;
        VariableName = variableName; // Сохраняем имя переменной
    }
}