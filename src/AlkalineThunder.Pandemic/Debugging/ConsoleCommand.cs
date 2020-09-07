using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AlkalineThunder.Pandemic.CommandLine;
using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic.Debugging
{
    public class ConsoleCommand
    {
        public string Name { get; }
        public string Description { get; }
        public MethodInfo MethodInfo { get; }
        public object This { get; }
        public string Usage { get; }
        
        public ConsoleCommand(string name, string description, MethodInfo method, object thisObject = null)
        {
            This = thisObject;
            Name = name;
            Description = description;
            MethodInfo = method;
            Usage = GetUsage();
        }

        private string GetUsage()
        {
            var parameters = MethodInfo.GetParameters();
            var sb = new StringBuilder();
            
            sb.Append(Name);

            foreach (var p in parameters)
            {
                sb.Append(" ");

                if (p.IsOptional)
                {
                    sb.Append($"[{p.Name}:{p.ParameterType.Name}]");
                }
                else
                {
                    sb.Append($"<{p.Name}:{p.ParameterType.Name}>");
                }
            }
            
            return sb.ToString();
        }

        
        public async Task Call(GameLoop ctx, string[] args)
        {
            var objs = new List<object>();
            var parameters = MethodInfo.GetParameters();

            await Task.Run(() =>
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];

                    if (i >= args.Length)
                        throw new ShellException(Usage);

                    var arg = args[i];

                    if (p.ParameterType == typeof(string))
                    {
                        objs.Add(arg);
                    }
                    else
                    {
                        objs.Add(JsonConvert.DeserializeObject(arg, p.ParameterType));
                    }
                }
            });
            
            await ctx.Invoke(() =>
            {
                MethodInfo.Invoke(This, objs.ToArray());
            });
        }
    }
}