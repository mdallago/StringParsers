using System;
using System.Linq.Expressions;

namespace StringParsers
{
    public abstract class SeparatorStringParser<T> : AbstractStringParser<T> where T : class,new()
    {
        private class SeparatorParserDefinition : PaserDefinition
        {
            public int Index { get; set; }
        }

        private readonly char separator;
        private string[] partes;

        protected SeparatorStringParser(char separator)
        {
            this.separator = separator;
        }

        protected DefinitionBuilder Define<TProperty>(Expression<Func<T, TProperty>> property, int index)
        {
            return AddDefinition(property, new SeparatorParserDefinition { Index = index });
        }

        protected override string GetValue(PaserDefinition definition)
        {
            return partes[((SeparatorParserDefinition)definition).Index];
        }

        protected override string GetString(string value)
        {
            return value + separator;
        }

        protected override void Initialize(string text)
        {
            partes = text.Split(separator);
        }
    }
}