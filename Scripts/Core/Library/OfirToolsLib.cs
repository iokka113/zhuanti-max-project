/** I’ve always struggled with manipulation on enums, it always look awkward and long.
 *  So I started collecting methods to make it easier.
 *  From MSDN:
 *      The enum keyword is used to declare an enumeration, a distinct type consisting of a set of named constants called the enumerator list.
 *      Every enumeration type has an underlying type, which can be any integral type except char.
 *      The default underlying type of the enumeration elements is int.
 *      By default, the first enumerator has the value 0, and the value of each successive enumerator is increased by 1
 *  Here are the list of methods supported:
 *      Any - Checks if the instance matches any of the values given (instead of (x & EnumType.A) != 0 || (x & EnymType.B) != 0, etc.).
 *      All – Checks if the instance matches all of the values given  (instead of (x & EnumType.A) != 0 && (x & EnymType.B) != 0, etc.).
 *      IsFlagged – Determines whether one or more bit fields are set in the current instance. (same as Enum.HasFlag – FrameWork 4.0).
 *      BaseType – Get the base-type of the instance.
 *      ToFlags – Yield all defined flags that is contained in the instance.
 *      Join – Yield all defined flags that is contained both instances.
 *      LeftOuterJoin – Yield all defined flags that is contained in the instance and not in the given value.
 *      RightOuterJoin – Yield all defined flags that is contained in the given value  and not in the instance.
 *      Parse – Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
 *      TryParse – Tries to convert the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
 *  Please use as you like.
 *  I appreciate comments and ideas.
 *  Enjoy,
 *  Ofir
 *
 *  I Like Enum (Enum’s extension methods) October 11, 2012
 *  https://ofirzeitoun.wordpress.com/2012/10/11/i-like-enum-enums-extension-methods/
 *  credit by OFIR ZEITOUN
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Ofir.Tools;
using Ofir.Tools.Reflection;

namespace Ofir.Tools
{
	public static class EnumExtensions
	{
		/// <summary>
		/// holds specific comparers for each enum type, to make comparison faster
		/// </summary>
		private readonly static Dictionary<TypeCode, Func<Enum, Enum, bool>> Comparers =
		new Dictionary<TypeCode, Func<Enum, Enum, bool>>(){
			{TypeCode.Byte,   (e1, e2) => (Convert.ToByte(e1)   & Convert.ToByte(e2))   != 0},
			{TypeCode.SByte,  (e1, e2) => (Convert.ToSByte(e1)  & Convert.ToSByte(e2))  != 0},
			{TypeCode.Int16,  (e1, e2) => (Convert.ToInt16(e1)  & Convert.ToInt16(e2))  != 0},
			{TypeCode.Int32,  (e1, e2) => (Convert.ToInt32(e1)  & Convert.ToInt32(e2))  != 0},
			{TypeCode.Int64,  (e1, e2) => (Convert.ToInt64(e1)  & Convert.ToInt64(e2))  != 0},
			{TypeCode.UInt16, (e1, e2) => (Convert.ToUInt16(e1) & Convert.ToUInt16(e2)) != 0},
			{TypeCode.UInt32, (e1, e2) => (Convert.ToUInt32(e1) & Convert.ToUInt32(e2)) != 0},
			{TypeCode.UInt64, (e1, e2) => (Convert.ToUInt64(e1) & Convert.ToUInt64(e2)) != 0},
		};

		/// <summary>
		/// check if <see cref="@this"/> contains any of enums in flags
		/// <remarks>instead of using:<code>(enum1 & enum2) != 0</code></remarks>
		/// </summary>
		/// <param name="this">enum to check</param>
		/// <param name="flags">list of flags to look for</param>
		/// <returns>true if at least one of the flags contained in @this; otherwise, false</returns>
		public static bool Any(this Enum @this, params Enum[] flags){
		return flags.Any(@enum => Contains(@this, @enum));
		}

		/// <summary>
		/// check if <see cref="@this"/> contains all of enums in flags
		/// </summary>
		/// <param name="this">enum to check</param>
		/// <param name="flags">list of flags to look for</param>
		/// <returns>true if all of the flags contained in @this; otherwise, false</returns>
		public static bool All(this Enum @this, params Enum[] flags){
		return flags.All(@enum => Contains(@this, @enum));
		}

		/// <summary>
		/// check if <see cref="@this"/> contains the <see cref="flag"/>
		/// </summary>
		/// <param name="this">enum to check</param>
		/// <param name="flag">flag to look for</param>
		/// <returns>true if @this contains flag</returns>
		private static bool Contains(Enum @this, Enum flag){
			return Comparers[@this.GetTypeCode()](@this, flag);
		}

		/// <summary>
		/// Check if the Enum declaration of <see cref="@this"/> has <see cref="FlagsAttribute"/>
		/// </summary>
		/// <param name="this">enum to check</param>
		/// <returns>true if the Enum declaration has <see cref="FlagsAttribute"/>; otherwise, false</returns>
		public static bool IsFlagged(this Enum @this){
			return @this.HasAttribute<FlagsAttribute>();
		}

		/// <summary>
		/// Get the base-type of <see cref="@this"/> as in Enum declaration
		/// </summary>
		/// <param name="this">enum to check</param>
		/// <returns>the underlying type of th enum</returns>
		public static Type BaseType(this Enum @this){
			Type res = null;
			switch (@this.GetTypeCode()){
				case TypeCode.SByte:
					res = typeof(sbyte);
					break;
				case TypeCode.Byte:
					res = typeof(byte);
					break;
				case TypeCode.Int16:
					res = typeof(Int16);
					break;
				case TypeCode.UInt16:
					res = typeof(UInt16);
					break;
				case TypeCode.Int32:
					res = typeof(Int32);
					break;
				case TypeCode.UInt32:
					res = typeof(UInt32);
					break;
				case TypeCode.Int64:
					res = typeof(Int16);
					break;
				case TypeCode.UInt64:
					res = typeof(UInt16);
					break;
				default:
					throw new ArgumentOutOfRangeException("this", @this, "This cannot accor");
			}
			return res;
		}

		/// <summary>
		/// Checks if <see cref="@this"/> is the same as <see cref="other"/>
		/// </summary>
		/// <param name="this">enum to check</param>
		/// <param name="other">another enum to check</param>
		/// <returns>true, if @this and other are of the same type</returns>
		public static bool IsSameTypeAs(this Enum @this, Enum other){
			return @this.GetType() == other.GetType();
		}

		/// <summary>
		/// Yield all defined flags that is contained in <see cref="@this"/>
		/// </summary>
		/// <param name="this">enum to itrate</param>
		/// <returns>enumerator that iterates each flag @this contains</returns>
		public static IEnumerable<Enum> ToFlags(this Enum @this){
			if (!@this.IsFlagged()){
				yield return @this;
			}
			else{
				Func<Enum, Enum, bool> comparer = Comparers[@this.GetTypeCode()];
				foreach(Enum flag in Enum.GetValues(@this.GetType())){
					if(comparer(flag, @this)){
						yield return flag;
					}
				}
			}
		}

		/// <summary>
		/// Yield all defined flags that is contained both in <see cref="@this"/> and in <see cref="other"/>
		/// </summary>
		/// <param name="this">enum to iterate </param>
		/// <param name="other">enum to join</param>
		/// <returns> enumerator that iterates each flag @this and also other contains</returns>
		public static IEnumerable<Enum> Join(this Enum @this, Enum other){
			Func<Enum, Enum, bool> comparer = Comparers[@this.GetTypeCode()];
			return @this.ToFlags().Where(flag => comparer(other, flag));
		}

		/// <summary>
		/// Yield all defined flags that is contained in <see cref="@this"/> and not in <see cref="other"/>
		/// </summary>
		/// <param name="this">enum to include</param>
		/// <param name="other">enum to exclude</param>
		/// <returns>enumarator that itterates each flag @this contains, but not containd in other</returns>
		public static IEnumerable<Enum> LeftOuterJoin(this Enum @this, Enum other){
			Func<Enum, Enum, bool> comparer = Comparers[@this.GetTypeCode()];
			return @this.ToFlags().Where(flag => !comparer(other, flag));
		}

		/// <summary>
		/// Yield all defined flags that is contained in <see cref="other"/> and not in <see cref="@this"/>
		/// </summary>
		/// <param name="this">enum to exclude</param>
		/// <param name="other">enum to include</param>
		/// <returns>enumarator that itterates each flag other contains, but not containd in @this</returns>
		public static IEnumerable<Enum> RightOuterJoin(this Enum @this, Enum other){
			return other.LeftOuterJoin(@this);
		}

		/// <summary>
		/// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
		/// A parameter specifies whether the operation is case-sensitive.
		/// </summary>
		/// <typeparam name="T">An enumeration type</typeparam>
		/// <param name="value">A string containing the name or value to convert</param>
		/// <param name="ignoreCase">true to ignore case; false to regard case</param>
		/// <returns>An object of type enumType whose value is represented by value</returns>
		public static T Parse<T>(this string value, bool ignoreCase = false) where T : struct{
			Type enumType = typeof (T);
			if(!enumType.IsEnum){
				throw new ArgumentException(string.Format("T ({0}) is not an Enum", enumType.Name), "T");
			}
			return (T)Enum.Parse(enumType, value, ignoreCase);
		}

		/// <summary>
		/// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
		/// A parameter specifies whether the operation is case-sensitive. The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <typeparam name="T">The enumeration type to which to convert value</typeparam>
		/// <param name="value">The string representation of the enumeration name or underlying value to convert</param>
		/// <param name="result">When this method returns, contains an object of type T whose value is represented by value. This parameter is passed uninitialized</param>
		/// <param name="default">Default value to save in result, if parse is failed</param>
		/// <param name="ignoreCase">true to ignore case; false to consider case</param>
		/// <returns>true if the value parameter was converted successfully; otherwise, false</returns>
		public static bool TryParse<T>(this string value, out T result, T @default = default(T), bool ignoreCase = false) where T : struct{
			bool res;
			try{
				result = value.Parse<T>(ignoreCase);
				res = true;
			}
			catch(Exception){
				result = @default;
				res = false;
			}
			return res;
		}
	}
}

namespace Ofir.Tools.Reflection
{
	public static class AttributeExtensions
	{
		public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute{
			return Attribute.GetCustomAttributes(memberInfo, typeof(T), inherit).Cast<T>();
		}

		public static IEnumerable<T> GetAttributes<T>(this object @this, bool inherit = false) where T : Attribute{
			return @this.GetType().GetCustomAttributes(typeof(T), inherit).Cast<T>();
		}

		public static bool HasAttribute<T>(this object @this, bool inherit = false) where T : Attribute{
			return GetAttributes<T>(@this, inherit).Any();
		}
	}
}

namespace Ofir.Example
{
    class Program
    {
        [Flags]
        private enum Ofir{
            A = 1,
            B = 2,
            C = 4,
        }

        static void Start(string[] args){
            Ofir ofir = Ofir.A | Ofir.B | Ofir.C;
            Console.WriteLine("ToFlags():");
            foreach (Ofir flag in ofir.ToFlags()){
                Console.WriteLine(flag);
            }

            ofir = Ofir.B | Ofir.C;
            bool b = ofir.All(Ofir.B, Ofir.A);
            Console.WriteLine("All(): {0}", b);

            Console.WriteLine("LeftOuterJoin():");
            foreach(Ofir onLeftOnly in ofir.LeftOuterJoin(Ofir.A | Ofir.B)){
                Console.WriteLine(onLeftOnly);
            }

            Ofir res = ofir ^ Ofir.C ^ Ofir.A;
            Console.WriteLine(ofir.All(Ofir.A, Ofir.B));

            ofir = "A,C".Parse<Ofir>();
            Console.WriteLine("Parse(): {0}", ofir);

            "A,B,c".TryParse(out ofir,ignoreCase:true);
            Console.WriteLine("TryParse(ignoreCase:true): {0}", ofir);
            Console.Read();
        }

        /*  Output:
            ToFlags():
            A
            B
            C
            All(): False
            LeftOuterJoin():
            C
            False
            Parse(): A, C
            TryParse(ignoreCase:true): A, B, C
        */
    }
}
