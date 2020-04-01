﻿using System.Collections;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/*
 * TODO when xplatform - this clsss should be removed and the AutoMapper extensions nuget
 * packsge should be included
 *
 * This source had to be included and hacked because when running functional tests
 * an exception was thrown with the following information:
 *   System.Reflection.ReflectionTypeLoadException : Unable to load one or more of the requested types.
 *   Could not load type 'System.Runtime.Remoting.Channels.IClientChannelSink' from assembly 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'.
 * plus half a dozen more in CookComputing and a bazillion in System.Web.
 * This souurce is from commit
 * https://github.com/AutoMapper/AutoMapper.Extensions.Microsoft.DependencyInjection/commit/a2e41e49101cdb5f7dfcc378f7724ac0ae3313bf
 * probably v5.0.1
 *
 * The hack marked below in the code prevents the exeption.
 *
 * The problem is caused by AutoMapper's loading all the types it can find in a scan of all loaded assemblies.
 * Unfortunately some of those types are referenced in the newtelligence legacy modules but are no where
 * to be found at runtime as we are using .net core which does not support the particular apis.  An exception is thrown.
 *
 * The question is why does this only happen wehn running FunctionalTests?
 * My guess is that Microsoft.NET.Test.Sdk which I'm pretty sure contains the entry point for FunctionalTests
 * loads all likely looking assemblies whereas the normal loading proecedure excludes any that are unreachable.  Does it lazy load?
 * I can attest that FunctionalTests has 381 loaded assemblies by the time AutoMapper does its stuff whereas
 * with the normal loadoing process there are only 101.
 */

namespace AutoMapper
{
	// **** this is a dasBlog adaptation of the original Automapper Extension class
	// to address problems with loading assemblies
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions to scan for AutoMapper classes and register the configuration, mapping, and extensions with the service collection
    /// - Finds <see cref="Profile"/> classes and initializes a new <see cref="MapperConfiguration" />
    /// - Scans for <see cref="ITypeConverter{TSource,TDestination}"/>, <see cref="IValueResolver{TSource,TDestination,TDestMember}"/>, <see cref="IMemberValueResolver{TSource,TDestination,TSourceMember,TDestMember}" /> and <see cref="IMappingAction{TSource,TDestination}"/> implementations and registers them as <see cref="ServiceLifetime.Transient"/>
    /// - Registers <see cref="IConfigurationProvider"/> as <see cref="ServiceLifetime.Singleton"/>
    /// - Registers <see cref="IMapper"/> as <see cref="ServiceLifetime.Scoped"/> with a service factory of the scoped <see cref="IServiceProvider"/>
    /// After calling AddAutoMapper you can resolve an <see cref="IMapper" /> instance from a scoped service provider, or as a dependency
    /// To use <see cref="QueryableExtensions.Extensions.ProjectTo{TDestination}(IQueryable,IConfigurationProvider, System.Linq.Expressions.Expression{System.Func{TDestination, object}}[])" /> you can resolve the <see cref="IConfigurationProvider"/> instance directly for from an <see cref="IMapper" /> instance.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IServiceProvider, IMapperConfigurationExpression> additionalInitAction)
        {
            return AddAutoMapperClasses(services, additionalInitAction, AppDomain.CurrentDomain.GetAssemblies());
        }

        private static readonly Action<IServiceProvider, IMapperConfigurationExpression> DefaultConfig = (sp,cfg) => {};

        private static IServiceCollection AddAutoMapperClasses(IServiceCollection services, Action<IServiceProvider, IMapperConfigurationExpression> additionalInitAction, IEnumerable<Assembly> assembliesToScan)
        {
            // Just return if we've already added AutoMapper to avoid double-registration
            if (services.Any(sd => sd.ServiceType == typeof(IMapper)))
                return services;

            additionalInitAction = additionalInitAction ?? DefaultConfig;
            assembliesToScan = assembliesToScan as Assembly[] ?? assembliesToScan.ToArray();

			// dasBlog hack ... only look at our modules...
	        DetectAssembliesRequiringExclusion(assembliesToScan);

			var allTypes = assembliesToScan
					.Where(a => a.GetName().Name.ToLower().StartsWith("dasblog"))
					.SelectMany(a => a.DefinedTypes)
					.ToArray();

			var profiles = allTypes
                .Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
	        
            void ConfigAction(IServiceProvider serviceProvider, IMapperConfigurationExpression cfg)
            {
                additionalInitAction(serviceProvider, cfg);

                foreach (var profile in profiles.Select(t => t.AsType()))
                {
                    cfg.AddProfile(profile);
                }
            }

            var openTypes = new[]
            {
                typeof(IValueResolver<,,>),
                typeof(IMemberValueResolver<,,,>),
                typeof(ITypeConverter<,>),
                typeof(IMappingAction<,>)
            };

            foreach (var type in openTypes.SelectMany(openType => allTypes
                .Where(t => t.IsClass 
                    && !t.IsAbstract 
                    && t.AsType().ImplementsGenericInterface(openType))))
            {
                services.AddTransient(type.AsType());
            }

            services.AddSingleton<IConfigurationProvider>(sp => new MapperConfiguration(c => ConfigAction(sp, c)));
            return services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService));
        }

	    private static void DetectAssembliesRequiringExclusion(IEnumerable<Assembly> assembliesToScan)
	    {
		    List<TypeInfo> typeList = new List<TypeInfo>();
		    foreach (Assembly ass in assembliesToScan)
		    {
			    try
			    {
				    foreach (TypeInfo ti in ass.GetTypes())
				    {
					    typeList.Add(ti);
				    }
			    }
			    catch (Exception e)
			    {
				    System.Diagnostics.Debug.WriteLine($"{ass.GetName().Name} FAILED: {e.Message}");
					// anything turning up herer should be included in excludedAssemblies above
			    }
		    }
	    }

	    private static bool ImplementsGenericInterface(this Type type, Type interfaceType)
        {
            return type.IsGenericType(interfaceType) || type.GetTypeInfo().ImplementedInterfaces.Any(@interface => @interface.IsGenericType(interfaceType));
        }

        private static bool IsGenericType(this Type type, Type genericType)
            => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == genericType;
    }
}
