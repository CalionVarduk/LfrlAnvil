namespace LfrlAnvil.TestExtensions;

public abstract class TestsBase
{
    private Fixture? _fixture;

    protected TestsBase(Fixture? fixture = null)
    {
        _fixture = fixture;
    }

    protected Fixture Fixture => _fixture ??= new Fixture();
}
