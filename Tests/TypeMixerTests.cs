﻿using NUnit.Framework;

namespace Concoct
{
    [TestFixture]
    public class TypeMixerTests
    {
        public abstract class FooBase
        {
            public int Bar() { return Foo() + Foo(); }
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
            Assert.That(mixed.Bar(), Is.EqualTo(42));
        }

        [Test]
        public void should_reuse_generated_class() {
            var first = TypeMixer<FooBase>.MixWith(new HalfFoo { FooReturns = 21 });
            var second = TypeMixer<FooBase>.MixWith(new HalfFoo { FooReturns = 21 });
            Assert.That(first.GetType(), Is.EqualTo(second.GetType()));
        }
    }
}