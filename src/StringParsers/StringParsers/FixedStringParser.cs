using System;
using System.Linq;
using System.Linq.Expressions;

namespace StringParsers
{
    public abstract class FixedStringParser<T> : AbstractStringParser<T> where T : class,new()
    {
        protected class FixedStringDefinitionBuilder
        {
            private readonly DefinitionBuilder builder;

            public FixedStringDefinitionBuilder(DefinitionBuilder builder)
            {
                this.builder = builder;
            }

            public FixedStringDefinitionBuilder WithValidator(Func<string, bool> validator)
            {
                builder.WithValidator(validator);
                return this;
            }

            public FixedStringDefinitionBuilder WithReadConverter<TProperty>(Func<string, TProperty> converter)
            {
                builder.WithReadConverter(converter);
                return this;
            }

            public FixedStringDefinitionBuilder WithWriteConverter<TProperty>(Func<TProperty, string> converter)
            {
                builder.WithWriteConverter(converter);
                return this;
            }

            public FixedStringDefinitionBuilder WithWriteLeftPadder(char @char)
            {
                builder.WithWriteConverter(LeftPadder<object>(((FixedParserDefinition)builder.Definition).Length, @char));
                return this;
            }

            public FixedStringDefinitionBuilder WithWriteRightPadder(char @char)
            {
                builder.WithWriteConverter(RightPadder<object>(((FixedParserDefinition)builder.Definition).Length, @char));
                return this;
            }

            public FixedStringDefinitionBuilder WithWriteZeroLeftPadder()
            {
                return WithWriteLeftPadder('0');
            }

            public FixedStringDefinitionBuilder WithWriteZeroRightPadder()
            {
                return WithWriteRightPadder('0');
            }

            public FixedStringDefinitionBuilder WithWriteSpaceLeftPadder()
            {
                return WithWriteLeftPadder(' ');
            }

            public FixedStringDefinitionBuilder WithWriteSpaceRightPadder()
            {
                return WithWriteRightPadder(' ');
            }

            private Func<TProperty, string> LeftPadder<TProperty>(int length, char @char)
            {
                return x =>
                {
                    var temp = x.ToString().PadLeft(length, @char);
                    return (temp.Length > length) ? temp.Substring(0,length) :temp;
                };
            }

            private Func<TProperty, string> RightPadder<TProperty>(int length, char @char)
            {
                return x =>
                {
                    var temp = x.ToString().PadRight(length, @char);
                    return (temp.Length > length) ? temp.Substring(0, length) : temp;
                };
            }
        }

        private class FixedParserDefinition:PaserDefinition
        {
            public int Start { get; set; }
            public int Length { get; set; }
        }

        private string data;

        protected FixedStringDefinitionBuilder Define<TProperty>(Expression<Func<T, TProperty>> property, int start, int length)
        {
            return new FixedStringDefinitionBuilder(AddDefinition(property, new FixedParserDefinition {Start = start, Length = length}));
        }

        protected override string GetValue(PaserDefinition definition)
        {
            return data.Substring(((FixedParserDefinition)definition).Start, ((FixedParserDefinition)definition).Length);
        }

        protected override string GetString(string value)
        {
            return value;
        }

        protected override void Initialize(string text)
        {
            data = text;
        }

        protected void ValidateDefinitions()
        {
            var def = Definitions.Take(1).Cast<FixedParserDefinition>().Single();
            int inicio = def.Start;
            int largo = def.Length;
            
            foreach (var definition in Definitions.Skip(1).Cast<FixedParserDefinition>())
            {
                if (definition.Start != inicio + largo)
                {
                    throw new StringParserException(string.Format("Error en la definicion de los campos. Campo:{0}", definition.Member));
                }
                inicio = definition.Start;
                largo = definition.Length;
            }
        }
    }
}