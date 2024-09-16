using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Json5Core
{
	internal static class InternalHelpers
	{
		public static bool IsGenericType(this Type t)
		{
			return t.IsGenericType;
		}

		public static bool IsEnum(this Type t)
		{
			return t.IsEnum;
		}

		public static bool IsPrimitive(this Type t)
		{
			return t.IsPrimitive;
		}

		public static bool IsValueType(this Type t)
		{
			return t.IsValueType;
		}

		public static bool IsClass(this Type t)
		{
			return t.IsClass;
		}

		public static bool IsAbstract(this Type t)
		{
			return t.IsAbstract;
		}

		public static bool IsInterface(this Type t)
		{
			return t.IsInterface;
		}

		public static object TryGetUninitializedObject(Type t)
		{
			return RuntimeHelpers.GetUninitializedObject(t);
		}

		public static byte[] TryGetMethodBodyIlByteArray(MethodInfo mi)
		{ 
			return mi.GetMethodBody()?.GetILAsByteArray() ?? [];
		}

		public static MemberInfo? ModuleResolveMember(Module m, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			return m.ResolveMember(metadataToken, genericTypeArguments, genericMethodArguments);
		}
	}
}
