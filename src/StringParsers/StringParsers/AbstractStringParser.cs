using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FastMember;

namespace StringParsers
{
    public abstract class AbstractStringParser<T> : IStringParser<T> where T : class,new()
    {
        protected class DefinitionBuilder
        {
            public PaserDefinition Definition { get; private set; }

            public DefinitionBuilder(PaserDefinition definition)
            {
                this.Definition = definition;
            }

            public DefinitionBuilder WithValidator(Func<string, bool> validator)
            {
                Definition.Validator = validator;
                return this;
            }

            public DefinitionBuilder WithReadConverter<TProperty>(Func<string, TProperty> converter)
            {
                Definition.Converter = converter;
                return this;
            }

            public DefinitionBuilder WithWriteConverter<TProperty>(Func<TProperty, string> converter)
            {
                Definition.WriteConverter = converter;
                return this;
            }
        }

        protected class PaserDefinition
        {
            public Func<string, bool> Validator { get; set; }
            public Delegate Converter { get; set; }
            public Delegate WriteConverter { get; set; }
            public MemberInfo Member { get; set; }
        }
        
        private readonly List<PaserDefinition> definitions = new List<PaserDefinition>();
        private readonly TypeAccessor accesor = TypeAccessor.Create(typeof(T));

        protected IEnumerable<PaserDefinition> Definitions
        {
            get { return definitions; }
        }

        private MemberInfo DecodeMemberAccessExpression<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
                return ((MemberExpression)expression.Body).Member;
            if (expression.Body.NodeType == ExpressionType.Convert && expression.Body.Type == typeof(TProperty))
                return ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member;
            throw new StringParserException(string.Format("Invalid expression type: Expected ExpressionType.MemberAccess, Found {0}", expression.Body.NodeType));
        }

        protected abstract string GetValue(PaserDefinition definition);
        protected abstract string GetString(string value);
        
        protected abstract void Initialize(string text);

        public T Parse(string text)
        {
            Initialize(text);
            var ret = new T();
            
            foreach (var definition in definitions)
            {
                var value = GetValue(definition);
                
                if ((definition.Validator != null) &&  (!definition.Validator(value)))
                {
                    throw new StringParserException(string.Format("{0} no valido: {1}", definition.Member.Name, value));
                }

                try
                {
                    var valueToSet = definition.Converter != null
                    ? definition.Converter.DynamicInvoke(value)
                    : Convert.ChangeType(value, ((PropertyInfo)definition.Member).PropertyType);

                    accesor[ret,definition.Member.Name] = valueToSet;
                    /*switch (definition.Member.MemberType)
                    {
                        case MemberTypes.Property:
                            ((PropertyInfo)definition.Member).SetValue(ret, valueToSet, null);
                            break;
                        case MemberTypes.Field:
                            ((FieldInfo)definition.Member).SetValue(ret, valueToSet);
                            break;
                    }*/
                }
                catch (Exception ex)
                {
                    throw new StringParserException(string.Format("{0} no valido: {1}", definition.Member.Name, value),ex);
                }
            }

            return ret;
        }

        public string Write(T data)
        {
            var sb = new StringBuilder();

            foreach (var definition in definitions)
            {
                object value = accesor[data,definition.Member.Name];
                /*switch (definition.Member.MemberType)
                {
                    case MemberTypes.Property:
                        value = ((PropertyInfo)definition.Member).GetValue(data, null);
                        break;
                    case MemberTypes.Field:
                        value = ((FieldInfo)definition.Member).GetValue(data);
                        break;
                }*/

                try
                {
                    var stringValue = definition.WriteConverter != null
                       ? (string)definition.WriteConverter.DynamicInvoke(value)
                       : value.ToString();

                    sb.Append(GetString(stringValue));
                }
                catch (Exception ex)
                {
                    throw new StringParserException(string.Format("{0} no valido: {1}", definition.Member.Name, value),ex);
                }
            }

            return sb.ToString();
        }

        protected DefinitionBuilder AddDefinition<TProperty>(Expression<Func<T, TProperty>> property, PaserDefinition definition)
        {
            definition.Member = DecodeMemberAccessExpression(property);
            definitions.Add(definition);
            return new DefinitionBuilder(definition);
        }
    }
}