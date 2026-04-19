namespace Web_Vue.Server.Validators;

/// <summary> TestRequestViewModel 驗證器 </summary>
public class TestRequestValidator : AbstractValidator<TestRequestViewModel>
{
    /// <summary> Constructor </summary>
    public TestRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name 為必填欄位");
    }
}
