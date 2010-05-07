using NUnit.Framework;

namespace Concoct
{
    [TestFixture]
    public class TypeMixerTests
    {
        public abstract class FooBase
        {
            public int TwoFoo() { return Foo() + Foo(); }
            protected abstract int Foo();
        }

        public class HalfFoo
        {
            public int FooReturns;
            public int Foo() { return FooReturns; }
        }

        [Test]
        public void MixWith_sample() {
            var mixed = TypeMixer<FooBase>.MixWith(new HalfFoo { FooReturns = 21 });
            Assert.That(mixed.TwoFoo(), Is.EqualTo(42));
        }

        [Test]
        public void should_reuse_generated_class() {
            var first = TypeMixer<FooBase>.MixWith(new HalfFoo { FooReturns = 21 });
            var second = TypeMixer<FooBase>.MixWith(new HalfFoo { FooReturns = 21 });
            Assert.That(first.GetType(), Is.EqualTo(second.GetType()));
        }

        public abstract class FooImpl
        {
            [MixerTarget]
            protected HalfFoo inner;

            public HalfFoo Inner { get { return inner; } }

            public int TwoFoo() { return Foo() + Foo(); }
            protected abstract int Foo();
        }
        
        [Test]
        public void should_use_target_field_if_marked(){
            var inner = new HalfFoo { FooReturns = 11 };
            var item = TypeMixer<FooImpl>.MixWith(inner);
            Assert.That(item.Inner, Is.SameAs(inner));
            Assert.That(item.TwoFoo(), Is.EqualTo(inner.Foo() * 2));
        }
    }
}
