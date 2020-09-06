using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using AlkalineThunder.Pandemic.Gui.Controls;

namespace AlkalineThunder.Pandemic.Gui.Markup
{
    /// <summary>
    /// Provides functionality for working with Pandemic Framework GUI Markup files.
    /// </summary>
    public static class GuiBuilder
    {
        /// <summary>
        /// Compiles a GUI markup file into an invocable function that returns a new instance
        /// of the control represented in the markup.
        /// </summary>
        /// <param name="ctx">Any object that belongs to the GUI system.</param>
        /// <param name="path">The path to the file to build.</param>
        /// <returns>A callable function that creates a new instance of the represented control.</returns>
        public static Func<Control> MakeBuilderFunction(IGuiContext ctx, string path)
        {
            var contentPath = ctx.SceneSystem.GameLoop.Content.RootDirectory;
            var filePath = Path.Combine(contentPath, path);

            var xml = File.ReadAllText(filePath);
            var builderFunction = ParseGuiMarkup(ctx, xml);

            return builderFunction;
        }
        
        private static Func<Control> ParseGuiMarkup(IGuiContext ctx, string markup)
        {
            var doc = new XmlDocument();
            doc.LoadXml(markup);

            var root = doc.DocumentElement;

            if (root.Name != "gui")
                throw new FormatException("This XML string does not define a GUI layout.");

            var exp = BuildGuiExpression(ctx, root.FirstChild);

            return exp.Compile();
        }

        private static Expression BuildControlExpression(IGuiContext ctx, XmlNode element)
        {
            // is this a text element?
            if (element.Name == "#text")
            {
                // Simply return a constant string.
                return Expression.Constant(element.Value.ProcessMarkupValue());
            }
            else if (element.Name == "#comment")
            {
                return BuildGuiExpression(ctx, element.NextSibling);
            }
            else
            {
                // Get the type of control to create
                var elementType = ctx.SceneSystem.GetMarkupElementType(element.Name) ??
                                  throw new FormatException($"Element '{element.Name}' isn't a valid GUI element.");

                var hasSetText = false;
                var hasSetContent = false;
                
                // Control's text property, if any
                var textProperty = elementType.GetProperty("Text");
                
                // Content property, if any
                var contentProperty =
                    elementType.GetProperties().FirstOrDefault(x => x.PropertyType == typeof(Control));
                
                // "AddChild" method, if any.
                var addChild = elementType.GetMethod("AddChild");

                // list of expressions to build the control
                var expressions = new List<Expression>();

                // "new Control();" - creates the new control object.
                var constructor = Expression.New(elementType.GetConstructor(Type.EmptyTypes));
                var vars = new List<ParameterExpression>();
                
                // Local variable representing the instance of the control we're building
                var instance = Expression.Parameter(elementType, "instance");

                vars.Add(instance);
                
                // start by creating a new instance of the control.
                expressions.Add(Expression.Assign(instance, constructor));

                // Now that we have an instance, let's start processing control properties.
                foreach (var attribute in element.Attributes.OfType<XmlAttribute>())
                {
                    var member = ctx.SceneSystem.GetMarkupProperty(elementType, attribute.Name) as PropertyInfo;

                    if (member != null)
                    {
                        var memberAccess = Expression.MakeMemberAccess(instance, member);
                        Expression value;
                        
                        if (member.PropertyType.IsEnum)
                        {
                            var enumValue = (int) Enum.Parse(member.PropertyType, attribute.Value.Trim());
                            value = Expression.Convert(Expression.Constant(enumValue), member.PropertyType);
                        }
                        else
                        {
                            var builder = ctx.SceneSystem.GetPropertyBuilder(member.PropertyType) ??
                                          throw new InvalidOperationException(
                                              $"Type '{member.PropertyType.Name}' is not supported by the gui builder.");
                            
                            value = Expression.Convert(builder.BuildValueExpression(attribute.Value),
                                member.PropertyType);
                        }

                        expressions.Add(Expression.Assign(memberAccess, value));
                        continue;
                    }
                    else
                    {
                        var parentElement = element.ParentNode;
                        if (parentElement != null)
                        {
                            var parentType = ctx.SceneSystem.GetMarkupElementType(parentElement.Name);
                            if (parentType != null)
                            {
                                var attachedPropertyInfo =
                                    ctx.SceneSystem.GetAttachedPropertyInfo(parentType, attribute.Name);

                                if (attachedPropertyInfo != null)
                                {
                                    var setAttachedMethod = elementType.GetMethod("SetAttachedProperty", 0,
                                        new[] {typeof(string), typeof(object)});
                                    var arg1 = Expression.Constant(attachedPropertyInfo.PropName);
                                    Expression arg2;

                                    if (attachedPropertyInfo.PropertyType.IsEnum)
                                    {
                                        var enumValue = (int)Enum.Parse(attachedPropertyInfo.PropertyType,
                                            attribute.Value.Trim());
                                        arg2 = Expression.Convert(Expression.Constant(enumValue),
                                            attachedPropertyInfo.PropertyType);
                                    }
                                    else
                                    {
                                        var builder =
                                            ctx.SceneSystem.GetPropertyBuilder(attachedPropertyInfo.PropertyType) ??
                                            throw new InvalidOperationException(
                                                $"Type '{attachedPropertyInfo.PropertyType.Name}' is not supported by the gui builder.");

                                        arg2 = Expression.Convert(builder.BuildValueExpression(attribute.Value.Trim()),
                                            attachedPropertyInfo.PropertyType);
                                    }

                                    var methodCall = Expression.Call(instance, setAttachedMethod, arg1, Expression.Convert(arg2, typeof(object)));

                                    expressions.Add(methodCall);

                                    continue;
                                }
                            }
                        }
                    }

                    throw new InvalidOperationException(
                        $"'{attribute.Name}': Attribute not supported by element '{element.Name}' or by it's parent as an attached property.");
                }
                
                // Now process all children.
                foreach (var childElement in element.ChildNodes.OfType<XmlNode>())
                {
                    if (childElement.Name == "#comment")
                        continue;
                    
                    var childExpression = BuildControlExpression(ctx, childElement);

                    // Text expression.
                    if (childExpression is ConstantExpression)
                    {
                        if (hasSetText)
                            throw new InvalidOperationException(
                                "Attempt to set the text of an element more than once.");
                        
                        if (textProperty == null)
                            throw new FormatException($"Element '{element.Name}' doesn't support text.");

                        var textMember = Expression.MakeMemberAccess(instance, textProperty);
                        var textAssign = Expression.Assign(textMember, childExpression);

                        expressions.Add(textAssign);
                        hasSetText = true;
                    }
                    else
                    {
                        // It's another control.
                        // Do we have an AddChild method?
                        if (addChild != null)
                        {
                            // Okay, then create the control as a variable.
                            var childVariable = Expression.Parameter(typeof(Control), Guid.NewGuid().ToString());
                            var lambdaCall = Expression.Invoke(childExpression);
                            var assignment = Expression.Assign(childVariable, lambdaCall);

                            vars.Add(childVariable);
                            
                            // Make sure it's in our builder code
                            expressions.Add(assignment);

                            // Now, let's call AddChild.
                            var addChildInvocation = Expression.Call(instance, addChild, childVariable);
                            expressions.Add(addChildInvocation);
                        }
                        else if (contentProperty != null)
                        {
                            if (hasSetContent)
                                throw new InvalidOperationException("Control only supports a single child.");

                            // This is easy.
                            
                            // First access the instance "Content" property.
                            var contentAccess = Expression.MakeMemberAccess(instance, contentProperty);
                            // Now create an invocation of the child builder
                            var invocation = Expression.Invoke(childExpression);
                            // Assign the result of the invocation to the content prop
                            var assignment = Expression.Assign(contentAccess, invocation);

                            expressions.Add(assignment);
                            
                            hasSetContent = true;
                        }
                        else
                        {
                            throw new InvalidOperationException("Control does not support children.");
                        }
                    }
                }

                expressions.Add(instance);
                
                // The function that builds the entirety of the control including its children.
                var block = Expression.Block(typeof(Control), vars,
                    expressions
                );

                return Expression.Lambda<Func<Control>>(block);
            }
        }
        
        private static Expression<Func<Control>> BuildGuiExpression(IGuiContext ctx, XmlNode element)
        {
            // Code that builds the root gui element.
            var rootBlock = BuildControlExpression(ctx, element);

            if (rootBlock is Expression<Func<Control>> ret)
                return ret;
            else
            {
                var textBlock = typeof(TextBlock);
                var ctor = Expression.New(textBlock);
                var instance = Expression.Parameter(textBlock, "root");

                var instantiator = Expression.Assign(instance, ctor);
                var textAccessor = Expression.MakeMemberAccess(instance, textBlock.GetProperty("Text"));
                var textSetter = Expression.Assign(textAccessor, rootBlock);

                var builder = Expression.Block(new[] {instance}, instantiator, textSetter, instance);

                return Expression.Lambda<Func<Control>>(builder);
            }
        }
        
        /// <summary>
        /// Builds the specified markup file and returns a new instance of the control represented
        /// by the markup.
        /// </summary>
        /// <param name="ctx">Any object that belongs to the GUI system.</param>
        /// <param name="path">The path to the file to build.</param>
        /// <returns>The instance of the newly built control.</returns>
        public static Control Build(this IGuiContext ctx, string path)
        {
            return MakeBuilderFunction(ctx, path)();
        }

        private static string ProcessMarkupValue(this string value)
        {
            var sb = new StringBuilder();
            var lines = value.Trim().Split('\n');
            var lastLineHadText = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (lastLineHadText)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine();
                    lastLineHadText = false;
                }
                else
                {
                    if (lastLineHadText)
                    {
                        sb.Append(" ");
                    }

                    sb.Append(line.Trim());
                    lastLineHadText = true;
                }
            }
            
            return sb.ToString();
        }
    }
}