using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace fastJSON
{
	//Helper methods for older runtime support (fastJSON5)
	internal static class InternalHelpers
	{
		public static bool IsGenericType(this Type t)
		{
#if NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
			return t.IsGenericType;
#else
			return t.GetTypeInfo().IsGenericType;
#endif
		}

		public static bool IsEnum(this Type t)
		{
#if NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
			return t.IsEnum;
#else
			return t.GetTypeInfo().IsEnum;
#endif
		}

		public static bool IsPrimitive(this Type t)
		{
#if NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
			return t.IsPrimitive;
#else
			return t.GetTypeInfo().IsPrimitive;
#endif
		}

		public static bool IsValueType(this Type t)
		{
#if NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
			return t.IsValueType;
#else
			return t.GetTypeInfo().IsValueType;
#endif
		}

		public static bool IsClass(this Type t)
		{
#if NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
			return t.IsClass;
#else
			return t.GetTypeInfo().IsClass;
#endif
		}

		public static bool IsAbstract(this Type t)
		{
#if NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
			return t.IsAbstract;
#else
			return t.GetTypeInfo().IsAbstract;
#endif
		}

		public static bool IsInterface(this Type t)
		{
#if NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
			return t.IsInterface;
#else
			return t.GetTypeInfo().IsInterface;
#endif
		}

#if !(NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER)
		public static bool IsAssignableFrom(this Type t, Type t2)
		{
			return t.GetTypeInfo().IsAssignableFrom(t2);
		}
#endif

#if !(NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER)
		public static Type LookupType(string name, params string[] hintAssembly)
		{
			//check in hint assemblies
			for (int i = 0; i < hintAssembly.Length; i++)
			{
				try
				{
					Assembly a = Assembly.Load(new AssemblyName(hintAssembly[i]));
					return a.GetType(name, true);
				}
				catch { }
			}
			//check in bcl
			return Type.GetType(name, true);
		}
#endif

#if !(NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER)
		private static class _GetUninitializedObjectHelper
		{
			public readonly static Func<Type, object> _GetUninitializedObject;

			//Attempt to use a real impl for GetUninitializedObject if available at runtime
			static _GetUninitializedObjectHelper()
			{
				try
				{
					var t = LookupType("System.Runtime.Serialization.FormatterServices", "netstandard", "System.Runtime.Serialization.Formatters", "mscorlib");
					var mi = t.GetMethod("GetUninitializedObject", new Type[] { typeof(Type) });
					var d = (Func<Type, object>)mi.CreateDelegate(typeof(Func<Type, object>));
					_GetUninitializedObject = d;
				}
				catch { }
			}
		}
#endif

		public static object TryGetUninitializedObject(Type t)
		{
#if NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
			return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(t);
#else
			var getter = _GetUninitializedObjectHelper._GetUninitializedObject;
			if (getter != null) return getter(t);
			else return Reflection.Instance.FastCreateInstance(t);
#endif
		}

#if !(NETFRAMEWORK || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER)
		//Attempt to dynamically load the following methods
		private static class _TryGetMethodBodyILByteArrayHelper
		{
			public static readonly Func<MethodInfo, object> _GetMethodBody;
			public static readonly Func<object, byte[]> _GetILAsByteArray;

			static _TryGetMethodBodyILByteArrayHelper()
			{
				try
				{
					var getMethodBody = typeof(MethodInfo).GetMethod("GetMethodBody", new Type[0]);
					var methodBody = getMethodBody.ReturnType;
					var getILAsByteArray = methodBody.GetMethod("GetILAsByteArray", new Type[0]);
					_GetMethodBody = (Func<MethodInfo, object>)getMethodBody.CreateDelegate(typeof(Func<MethodInfo, object>));
					var getILAsByteArray2 = getILAsByteArray.CreateDelegate(typeof(Func<,>).MakeGenericType(methodBody, typeof(byte[])));
					var paramEx = Expression.Parameter(typeof(object));
					Expression e = Expression.Call(Expression.TypeAs(paramEx, methodBody), getILAsByteArray);
					_GetILAsByteArray = Expression.Lambda<Func<object, byte[]>>(e, paramEx).Compile();
				}
				catch { }
			}
		}
#endif

		public unsafe static byte[] TryGetMethodBodyILByteArray(MethodInfo mi)
		{
#if NETFRAMEWORK || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
			return mi.GetMethodBody()?.GetILAsByteArray() ?? new byte[0];
#else
			if (_TryGetMethodBodyILByteArrayHelper._GetMethodBody == null || _TryGetMethodBodyILByteArrayHelper._GetILAsByteArray == null) return null;
			var body = _TryGetMethodBodyILByteArrayHelper._GetMethodBody(mi);
			if (body == null) return new byte[0];
			return _TryGetMethodBodyILByteArrayHelper._GetILAsByteArray(body) ?? new byte[0];
#endif
		}

#if !(NETFRAMEWORK || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER)
		private static class _ModuleResolveMemberHelper
		{
			public static readonly Func<Module, int, Type[], Type[], MemberInfo> _ResolveMember;

			static _ModuleResolveMemberHelper()
			{
				try
				{
					var method = typeof(Module).GetMethod("ResolveMember", new Type[] { typeof(int), typeof(Type[]), typeof(Type[]) });
					_ResolveMember = (Func<Module, int, Type[], Type[], MemberInfo>)method.CreateDelegate(typeof(Func<Module, int, Type[], Type[], MemberInfo>));
				}
				catch { }
			}
		}
#endif

		public static bool HasModuleResolveMember
		{
			get
			{
#if NETFRAMEWORK || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
				return true;
#else
				return _ModuleResolveMemberHelper._ResolveMember != null;
#endif
			}
		}

		public static MemberInfo ModuleResolveMember(Module m, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
#if NETFRAMEWORK || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
			return m.ResolveMember(metadataToken, genericTypeArguments, genericMethodArguments);
#else
			return _ModuleResolveMemberHelper._ResolveMember(m, metadataToken, genericTypeArguments, genericMethodArguments);
#endif
		}
	}
}
