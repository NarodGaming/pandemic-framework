using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AlkalineThunder.Pandemic
{
    public static class ModuleLoader
    {
        public static IEnumerable<EngineModule> LoadModules(GameLoop ctx, Assembly ass)
        {
            var types = FindModulesInAssembly(ass);

            foreach (var module in LoadModules(ctx, types))
            {
                yield return module;
            }

            GameUtils.Log("Successfully loaded modules in assembly.");
        }
        
        private static IEnumerable<Type> FindModulesInAssembly(Assembly ass)
        {
            GameUtils.Log("Finding modules in " + ass.FullName);
            
            foreach (var type in ass.GetTypes())
            {
                if (type.BaseType == typeof(EngineModule) && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    GameUtils.Log(" -> Found: " + type.FullName);
                    yield return type;
                }
            }
        }

        private static IEnumerable<Type> GetRequirements(Type type)
        {
            var attributes = type.GetCustomAttributes(false)
                .OfType<RequiresModuleAttribute>();

            foreach (var attr in attributes)
            {
                if (attr.RequiredModuleType == type)
                    throw new ModuleException($"Type {type.FullName} requires itself.");

                yield return attr.RequiredModuleType;
            }
        }

        private static IEnumerable<EngineModule> LoadModules(GameLoop ctx, IEnumerable<Type> types)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            if (types == null)
                throw new ArgumentNullException(nameof(types));

            if (types.Any(x => x.BaseType != typeof(EngineModule)))
                throw new InvalidOperationException("Attempted to load types that are not engine modules.");

            var loadQueue = new List<Type>();
            
            foreach (var type in types)
            {
                GameUtils.Log($" -> Checking for circular dependencies...");
                DetectCircularDependencies(type, GetRequirements(type));

                foreach (var req in CollapseRequirements(type))
                {
                    if (!loadQueue.Contains(req))
                        loadQueue.Add(req);
                }

                if (!loadQueue.Contains(type))
                    loadQueue.Add(type);
            }

            GameUtils.Log("Loading modules now...");

            foreach (var item in loadQueue)
            {
                // No need to load mods that are already loaded
                if (ctx.IsModuleActive(item))
                    continue;
                
                var instance = (EngineModule) Activator.CreateInstance(item, null);

                instance.Register(ctx);

                GameUtils.Log($" -> Loaded: {item.Name}");
                
                yield return instance;
            }
        }

        private static IEnumerable<Type> CollapseRequirements(Type type)
        {
            foreach (var req in GetRequirements(type))
            {
                foreach (var cReq in CollapseRequirements(req))
                    yield return cReq;
                yield return req;
            }
        }
        
        private static void DetectCircularDependencies(Type type, IEnumerable<Type> requirements)
        {
            foreach (var req in requirements)
            {
                DetectCircularDependencies(type, GetRequirements(req));
                
                if (GetRequirements(req).Any(x => x == type))
                    throw new ModuleException($"Circular dependency between {type.FullName} and {req.FullName}.");
            }
        }

        public static bool InheritsFrom(this Type type, Type baseType)
        {
            var b = type;
            while (b != null)
            {
                if (b == baseType)
                    return true;
                b = b.BaseType;
            }

            return false;
        }

        internal static IEnumerable<EngineModule> LoadThirdPartyModules(GameLoop ctx)
        {
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (ass != ctx.GetType().Assembly)
                {
                    foreach (var mod in LoadModules(ctx, ass))
                        yield return mod;
                }
            }
        }
    }
}

