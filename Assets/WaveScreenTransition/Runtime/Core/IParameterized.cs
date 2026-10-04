namespace Wave.ScreenTransition
{
    public interface IParameterized<in TParameter>
    {
        void SetParameter(TParameter parameter);
    }
}
