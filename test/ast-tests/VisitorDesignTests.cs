namespace ast_tests;

public class VisitorDesignTests
{
  [Fact]
  public void test1()
  {
    var o = new SomeClass();
    var vo = new SomeVisitor();
    o.Accept(vo);
  }

}


public abstract partial record BaseClass : IVisitable
{
  public abstract void Accept(IVisitor visitor);
}

public partial record SomeClass : BaseClass
{
  public override void Accept(IVisitor visitor)
  {
    visitor.OnEnter(this);
    visitor.OnLeave(this);
  }
}

public static class SomeClassExtensions
{

}

public interface IVisitor
{
  public void OnEnter<T>(T ctx);
  public void OnLeave<T>(T ctx);
}

public interface IVisitable
{
  void Accept(IVisitor visitor);
}

public partial class SomeVisitor : IVisitor
{
  public void OnEnter<T>(T ctx)
  {
    // Default OnEnter behavior (no debug output)
  }

  public void OnLeave<T>(T ctx)
  {
    // Default OnLeave behavior (no debug output)
  }
}
