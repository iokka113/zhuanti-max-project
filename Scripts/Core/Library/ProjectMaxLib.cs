using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ProjectMax.CSharp
{
	/// <summary>物件函式庫</summary>
	public static class ObjectLib
	{
		/// <summary>預設值</summary>
		public static T Default<T>(){
			return default(T);
		}

		/// <summary>型別</summary>
		public static Type Type<T>(){
			return typeof(T);
		}

		/// <summary>將物件序列化為位元組陣列</summary>
		public static byte[] Serialize(object obj){
			using(MemoryStream stream = new MemoryStream()){
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, obj);
				return stream.ToArray();
			}
		}

		/// <summary>將位元組陣列反序列化為物件</summary>
		public static object Deserialize(byte[] bytes){
			using(MemoryStream stream = new MemoryStream()){
				BinaryFormatter formatter = new BinaryFormatter();
				stream.Write(bytes, 0, bytes.Length);
				stream.Seek(0, SeekOrigin.Begin);
				return formatter.Deserialize(stream);
			}
		}
	}

	/// <summary>列舉函式庫</summary>
	public static class EnumLib
	{
		[Flags]
		private enum TestFlags
		{
			None  = 0,      // 0000 = 0
			FlagA = 1 << 0, // 0001 = 1
			FlagB = 1 << 1, // 0010 = 2
			FlagC = 1 << 2, // 0100 = 4
			FlagD = 1 << 3, // 1000 = 8
		}
	}

	/// <summary>集合函式庫</summary>
	public static class CollectLib
	{
		/// <summary>檢查集合是否為空值或空參照</summary>
		public static bool IsNullOrEmpty(this ICollection collect){
			return collect == null || collect.Count == 0;
		}

		/// <summary>去除列表中的重複項</summary>
		/// <returns>沒有重複項的新列表</returns>
		public static List<T> Deduplicate<T>(this List<T> list){
			return list.Distinct().ToList();
		}

		/// <summary>嘗試從物件陣列中取值並轉換類型</summary>
		/// <returns>是否成功取值並轉換至該類型</returns>
		public static bool TryGetValue<T>(this object[] array, int index, out T value){
			if(array == null){
				throw new ArgumentNullException(nameof(array));
			}
			try{
				value = (T)array[index];
				return true;
			}
			catch{
				value = default(T);
				return false;
			}
		}

		/// <summary>嘗試將指定的索引鍵和值新增至字典</summary>
		/// <returns>指定的索引鍵和值是否已成功新增至字典</returns>
		/// <remarks>
		/// 當索引鍵存在於字典中時不執行任何動作<br/>
		/// **不會覆寫索引鍵當前的值**<br/>
		/// </remarks>
		public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict,
		TKey key, TValue value){
			if(dict == null){
				throw new ArgumentNullException(nameof(dict));
			}
			if(dict.ContainsKey(key)){
				return false;
			}
			else{
				dict.Add(key, value);
				return true;
			}
		}
	}

	/// <summary>字串函式庫</summary>
	public static class StringLib
	{
		/// <summary>當前環境換行符</summary>
		public static string n => Environment.NewLine;

		/// <summary>檢查字串是否為空值或空參照</summary>
		public static bool IsNullOrEmpty(this string str){
			return string.IsNullOrEmpty(str);
		}

		/// <summary>過濾跳脫序列</summary>
		public static string FilterEscape(string str){
			if(str == null){
				throw new ArgumentNullException(nameof(str));
			}
			string pattern = @"[\t\v\r\n\s]";
			return Regex.Replace(str, pattern, string.Empty);
		}

		/// <summary>過濾非法字符</summary>
		public static string FilterIllegal(this string str, List<string> illegal){
			if(str == null){
				throw new ArgumentNullException(nameof(str));
			}
			if(illegal == null){
				return str.Trim();
			}
			else{
				StringBuilder sb = new StringBuilder(str);
				foreach(string i in illegal){
					sb.Replace(i, new string('*', i.Length));
				}
				return sb.ToString().Trim();
			}
		}

		/// <summary>過濾非法字符</summary>
		public static string FilterIllegal(this string str, params string[] illegal){
			return FilterIllegal(str, illegal.ToList());
		}

		/// <summary>取得MD5雜湊值</summary>
		public static string GetMD5(this byte[] bytes){
			MD5 cryptoMD5 = MD5.Create();
			byte[] hash = cryptoMD5.ComputeHash(bytes);
			return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
		}

		/// <summary>取得MD5雜湊值</summary>
		public static string GetMD5(this string str){
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			return GetMD5(bytes);
		}

		/// <summary>取得隨機字串</summary>
		public static string RandomString(int length){
			return Guid.NewGuid().ToByteArray().GetMD5().Substring(0, length);
		}
	}

	/// <summary>平面角</summary>
	[Serializable]
	public struct Angle
	{
		public const double PI = Math.PI;

		public enum Unit { Degree, Radian, Gradian, Turn }

		/// <summary>建立一個平面角</summary>
		/// <param name="value">角的數值</param>
		/// <param name="unit">角的單位</param>
		public Angle(double value, Unit unit = Unit.Degree){
			this.value = value;
			_unit = unit;
		}

		/// <summary>角的數值</summary>
		public double value { get; set; }

		private Unit _unit;

		/// <summary>角的單位</summary>
		public Unit unit{
			get{
				return _unit;
			}
			set{
				this.value = Convert(this.value, _unit, value);
				_unit = value;
			}
		}

		#region preset

		public static Angle deg90 => new Angle(90d, Unit.Degree);
		public static Angle deg180 => new Angle(180d, Unit.Degree);
		public static Angle deg270 => new Angle(270d, Unit.Degree);
		public static Angle radPI => new Angle(PI, Unit.Radian);
		public static Angle rad2PI => new Angle(PI * 2d, Unit.Radian);
		public static Angle rad05PI => new Angle(PI * 0.5d, Unit.Radian);
		public static Angle turn1 => new Angle(1d, Unit.Turn);
		public static Angle turn05 => new Angle(0.5d, Unit.Turn);

		#endregion

		/// <summary>角的角度值</summary>
		public double degrees => Convert(value, _unit, Unit.Degree);

		/// <summary>角的徑度值</summary>
		public double radians => Convert(value, _unit, Unit.Radian);

		/// <summary>角的梯度值</summary>
		public double gradians => Convert(value, _unit, Unit.Gradian);

		/// <summary>角的轉值</summary>
		public double turns => Convert(value, _unit, Unit.Turn);

		/// <summary>將數值轉換至新單位</summary>
		/// <param name="value">原始數值</param>
		/// <param name="fromUnit">原始單位</param>
		/// <param name="toUnit">目標單位</param>
		/// <returns>轉換後的目標數值</returns>
		public static double Convert(double value, Unit fromUnit, Unit toUnit){
			if(fromUnit == Unit.Degree){
				if(toUnit == Unit.Radian){
					return value / 180d * PI;
				}
				else if(toUnit == Unit.Gradian){
					return value / 360d * 400d;
				}
				else if(toUnit == Unit.Turn){
					return value / 360d;
				}
				else{
					return value;
				}
			}
			else if(fromUnit == Unit.Radian){
				if(toUnit == Unit.Degree){
					return value / PI * 180d;
				}
				else if(toUnit == Unit.Gradian){
					return value / PI / 2d * 400d;
				}
				else if(toUnit == Unit.Turn){
					return value / PI / 2d;
				}
				else{
					return value;
				}
			}
			else if(fromUnit == Unit.Gradian){
				if(toUnit == Unit.Degree){
					return value / 400d * 360d;
				}
				else if(toUnit == Unit.Radian){
					return value / 400d * 2d * PI;
				}
				else if(toUnit == Unit.Turn){
					return value / 400d;
				}
				else{
					return value;
				}
			}
			else if(fromUnit == Unit.Turn){
				if(toUnit == Unit.Degree){
					return value * 360d;
				}
				else if(toUnit == Unit.Radian){
					return value * 2d * PI;
				}
				else if(toUnit == Unit.Gradian){
					return value * 400d;
				}
				else{
					return value;
				}
			}
			else{
				return value;
			}
		}

		public string ToString(Unit unit,
		string format = null, IFormatProvider provider = null){
			double fix = unit == Unit.Radian ? (value / PI) : value;
			string suffix = string.Empty;
			if(unit == Unit.Degree) suffix = "°";
			else if(unit == Unit.Radian) suffix = "π";
			else if(unit == Unit.Gradian) suffix = "gd";
			else if(unit == Unit.Turn) suffix = "x";
			return fix.ToString(format, provider) + suffix;
		}

		public string ToString(string format = null, IFormatProvider provider = null){
			return ToString(unit, format, provider);
		}

		public override string ToString(){
			return ToString(unit);
		}

		public override int GetHashCode(){
			return degrees.GetHashCode();
		}

		public override bool Equals(object obj){
			if(obj == null || GetType() != obj.GetType()){
				return base.Equals(obj);
			}
			return GetHashCode() == obj.GetHashCode();
		}

		#region operator

		public static implicit operator string(Angle angle){
			return angle.ToString();
		}

		public static bool operator ==(Angle left, Angle right){
			// overloads reference equality to value equality
			return Equals(left, right);
		}

		public static bool operator !=(Angle left, Angle right){
			// overloads reference equality to value equality
			return !Equals(left, right);
		}

		public static Angle operator +(Angle left, Angle right){
			right.unit = left.unit;
			return new Angle(left.value + right.value, left.unit);
		}

		public static Angle operator +(Angle angle, double value){
			return new Angle(angle.value + value, angle.unit);
		}

		public static Angle operator -(Angle left, Angle right){
			right.unit = left.unit;
			return new Angle(left.value - right.value, left.unit);
		}

		public static Angle operator -(Angle angle, double value){
			return new Angle(angle.value - value, angle.unit);
		}

		public static Angle operator *(Angle left, Angle right){
			right.unit = left.unit;
			return new Angle(left.value * right.value, left.unit);
		}

		public static Angle operator *(Angle angle, double value){
			return new Angle(angle.value * value, angle.unit);
		}

		public static Angle operator /(Angle left, Angle right){
			right.unit = left.unit;
			return new Angle(left.value / right.value, left.unit);
		}

		public static Angle operator /(Angle angle, double value){
			return new Angle(angle.value / value, angle.unit);
		}

		public static bool operator <(Angle left, Angle right){
			return left.degrees < right.degrees;
		}

		public static bool operator >(Angle left, Angle right){
			return left.degrees > right.degrees;
		}

		public static bool operator <=(Angle left, Angle right){
			return left.degrees <= right.degrees;
		}

		public static bool operator >=(Angle left, Angle right){
			return left.degrees >= right.degrees;
		}

		#endregion
	}

	/// <summary>隨機不重複整數陣列</summary>
	public class NonRepeating
	{
		private readonly int _start;
		private readonly int _count;

		private int[] _array = null;
		private int _index = 0;

		/// <summary>隨機不重複整數陣列</summary>
		/// <param name="start">首項</param>
		/// <param name="count">項數</param>
		public NonRepeating(int start, int count){
			_start = start;
			_count = count;
			_array = New(_start, _count);
			_index = 0;
		}

		private readonly object _lock = new object();

		/// <summary>下一項</summary>
		/// <remarks>
		/// 當陣列中有下一項時返回陣列中的下一項<br/>
		/// 當沒有下一項時重新建立陣列並返回第一項<br/>
		/// </remarks>
		public int next{
			get{
				lock(_lock){
					if(_index == _array.Length){
						_array = New(_start, _count);
						_index = 0;
					}
					int value = _array[_index];
					_index += 1;
					return value;
				}
			}
		}

		/// <summary>建立新的隨機陣列</summary>
		/// <param name="start">首項</param>
		/// <param name="count">項數</param>
		private static int[] New(int start, int count){
			System.Random r = new System.Random(Guid.NewGuid().GetHashCode());
			return Enumerable.Range(start, count).OrderBy(o => r.Next()).ToArray<int>();
		}
	}
}

namespace ProjectMax.Unity
{
	/// <summary>組件函式庫</summary>
	public static class CmptLib
	{
		/// <summary>尋找帶有標籤的遊戲物件並取得特定組件</summary>
		public static T FindWithTag<T>(string tag) where T : Component{
			try{
				return GameObject.FindWithTag(tag)?.GetComponent<T>();
			}
			catch(UnityException e){
				throw e;
			}
		}
	}

	/// <summary>單例模式靜態存取</summary>
	public static class Singleton<T> where T : Component
	{
		/// <summary>單例組件</summary>
		public static T instance { get; private set; } = null;

		private static readonly object _threadLock = new object();

		/// <summary>設定單例組件</summary>
		/// <returns>Singleton.instance</returns>
		/// <remarks>
		/// **僅透過 Singleton.instance 來取得組件的單例**<br/>
		/// 當組件單例為空時設定此組件為單例並執行初始化方法<br/>
		/// 當組件單例不為空時銷毀此組件及附加此組件的遊戲物件<br/>
		/// 可選 dontDestroy: 載入場景時是否保留附加單例的遊戲物件<br/>
		/// </remarks>
		public static T SetInstance(T self, bool dontDestroy = false, Action init = null){
			if(instance){
				if(instance != self){
					GameObject.Destroy(self.gameObject);
				}
			}
			else{
				lock(_threadLock){
					if(instance){
						if(instance != self){
							GameObject.Destroy(self.gameObject);
						}
					}
					else{
						instance = self;
						if(dontDestroy){
							GameObject.DontDestroyOnLoad(instance.gameObject);
						}
						Debug.Log($"singleton init {instance.GetType()}");
						init?.Invoke();
					}
				}
			}
			return instance;
		}
	}

	/// <summary>計時器</summary>
	public class UnityTimer
	{
		/// <summary>是否已開始計時</summary>
		public bool isTiming { get; private set; }

		/// <summary>計時結束時間</summary>
		private float _timesUp;

		/// <summary>檢查計時器狀態</summary>
		/// <param name="condition">開始計時條件</param>
		/// <param name="time">計時時間(秒)</param>
		/// <param name="onTimerStart">當計時開始時</param>
		/// <param name="onTimesUp">當計時結束時</param>
		/// <remarks>
		/// 當計時條件為真時, 呼叫一次 onTimerStart 並開始計時<br/>
		/// 當計時時間結束後, 呼叫一次 onTimesUp 等待下次計時<br/>
		/// 計時途中當條件再次為真時，不重複計時<br/>
		/// **將此函數置於 Update() 或 FixedUpdate() 中以循環檢查**<br/>
		/// </remarks>
		public void Timing(bool condition, float time,
		Action onTimerStart = null, Action onTimesUp = null){
			if(isTiming){
				if(Time.time > _timesUp){
					isTiming = false;
					onTimesUp?.Invoke();
				}
			}
			else{
				if(condition){
					isTiming = true;
					_timesUp = Time.time + time;
					onTimerStart?.Invoke();
				}
			}
		}

		/// <summary>重設計時器</summary>
		public void Reset(){
			isTiming = false;
			_timesUp = 0f;
		}
	}

	/// <summary>渲染函式庫</summary>
	public static class RenderLib
	{
		/// <summary>轉換為富文字</summary>
		/// <param name="b">粗體</param>
		/// <param name="i">斜體</param>
		/// <param name="c">顏色</param>
		/// <param name="s">大小</param>
		/// <param name="m">材質</param>
		public static string ToRichText(this string str, bool b = false, bool i = false,
		string c = null, int s = -1, int m = -1){
			str = b ? $"<b>{str}</b>" : str;
			str = i ? $"<i>{str}</i>" : str;
			str = s > -1 ? $"<size={s}>{str}</size>" : str;
			str = m > -1 ? $"<material={m}>{str}</material>" : str;
			str = c != null && c != string.Empty ? $"<color={c}>{str}</color>" : str;
			return str;
		}

		/// <summary>轉換為網頁用色碼</summary>
		/// <remarks>
		/// 當 alpha 啟用時返回格式為 "#RRGGBBAA" 的16進制色碼<br/>
		/// 當 alpha 禁用時返回格式為 "#RRGGBB" 的16進制色碼<br/>
		/// </remarks>
		public static string ToHexCode(this Color color, bool alpha = true){
			return alpha ?
			$"#{ColorUtility.ToHtmlStringRGBA(color)}" : $"#{ColorUtility.ToHtmlStringRGB(color)}";
		}

		/// <summary>在遊戲視圖內渲染二維碰撞器</summary>
		public static void DrawCollider2D(this BoxCollider2D collider, LineRenderer renderer,
		Material material, Color color, float widthMultiplier = 0.05f, int orderInLayer = 0){
			float sizeX = collider.size.x / 2f;
			float sizeY = collider.size.y / 2f;
			Vector2 offset = collider.offset;
			Vector3[] positions = new Vector3[4]{
				collider.transform.TransformPoint(new Vector2(+sizeX, +sizeY) + offset),
				collider.transform.TransformPoint(new Vector2(-sizeX, +sizeY) + offset),
				collider.transform.TransformPoint(new Vector2(-sizeX, -sizeY) + offset),
				collider.transform.TransformPoint(new Vector2(+sizeX, -sizeY) + offset),
			};
			renderer.material = material;
			renderer.startColor = color;
			renderer.endColor = color;
			renderer.widthMultiplier = widthMultiplier;
			renderer.startWidth = widthMultiplier;
			renderer.endWidth = widthMultiplier;
			renderer.loop = true;
			renderer.sortingOrder = orderInLayer;
			renderer.positionCount = positions.Length;
			renderer.SetPositions(positions);
		}

		/// <summary>在遊戲視圖內渲染二維碰撞器</summary>
		public static void DrawCollider2D(this CircleCollider2D collider, LineRenderer renderer,
		Material material, Color color, float widthMultiplier = 0.05f, int orderInLayer = 0){
			int segments = 360;
			float radius = collider.radius;
			Vector2 offset = collider.offset;
			Vector3[] positions = new Vector3[segments];
			for(int i = 0; i < segments; i++){
				positions[i] = collider.transform.TransformPoint(
				TransLib.AngleToVector2((float)i) * radius + offset);
			}
			renderer.material = material;
			renderer.startColor = color;
			renderer.endColor = color;
			renderer.widthMultiplier = widthMultiplier;
			renderer.startWidth = widthMultiplier;
			renderer.endWidth = widthMultiplier;
			renderer.loop = true;
			renderer.sortingOrder = orderInLayer;
			renderer.positionCount = segments;
			renderer.SetPositions(positions);
		}
	}

	/// <summary>浮點函式庫</summary>
	public static class FloatLib
	{
		/// <summary>在指定最小值和最大值區間中正規化浮點數</summary>
		/// <returns>0 到 1 之間的正規化結果</returns>
		public static float Normalize(this float origin, float min, float max){
			return (Mathf.Clamp(origin, min, max) - min) / (max - min);
		}

		/// <summary>反正規化 0 到 1 之間的正規化浮點數</summary>
		/// <returns>指定最小值和最大值區間中的反正規化結果</returns>
		public static float Denormalize(this float normalized, float min, float max){
			return (Mathf.Clamp(normalized, 0f, 1f) * (max - min) + min);
		}

		/// <summary>隨時間推移將一個值逐漸改變為所需目標。</summary>
		/// <param name="current">當前位置。</param>
		/// <param name="target">嘗試達到的目標。</param>
		/// <param name="currentVelocity">當前速度，此值由函數在每次調用時進行修改。</param>
		/// <param name="smoothTime">達到目標所需的近似時間。值越小，達到目標的速度越快。</param>
		/// <param name="maxSpeed">可以選擇允許限制最大速度。</param>
		/// <param name="deltaTime">自上次調用此函數以來的時間。默認情況下為 Time.deltaTime。</param>
		/// <remarks>
		/// 值通過某個類似於彈簧-阻尼的函數（它從不超過目標）進行平滑。<br/>
		/// 該函數可以用於平滑任何類型的值、位置、顏色、標量。<br/>
		/// **此函數內置於 UnityEngine.Mathf 庫**<br/>
		/// </remarks>
		public static float SmoothDamp(float current, float target, ref float currentVelocity,
		float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = default){
			if(deltaTime == default(float)){
				deltaTime = Time.deltaTime;
			}
			return Mathf.SmoothDamp(current, target, ref currentVelocity,
			smoothTime, maxSpeed, deltaTime);
		}

		/// <summary>數值平滑處理</summary>
		/// <param name="current">當前數值</param>
		/// <param name="target">目標數值</param>
		/// <param name="offset">偏移量</param>
		/// <param name="error">容許誤差</param>
		/// <param name="deltaTime">影格時間</param>
		/// <returns>不超過目標數值的平滑後的數值</returns>
		/// <remarks>
		/// 將當前數值往目標數值每秒偏移一個偏移量<br/>
		/// **如果當前數值與目標數值的差小於容許誤差時傳回目標數值**<br/>
		/// </remarks>
		public static float SmoothDamp(float current, float target, float offset,
		float error = 0f, float deltaTime = default){
			if(Mathf.Abs(current - target) < Mathf.Abs(error)){
				return target;
			}
			if(deltaTime == default(float)){
				deltaTime = Time.deltaTime;
			}
			if(target > current){
				current += offset * deltaTime;
				return current > target ? target : current;
			}
			if(target < current){
				current -= offset * deltaTime;
				return current < target ? target : current;
			}
			return current;
		}
	}

	/// <summary>變換函式庫</summary>
	public static class TransLib
	{
		/// <summary>座標軸向</summary>
		public enum Axis { X, Y, Z, W }

		/// <summary>游標位置</summary>
		public static Vector3 cursorPos => Input.mousePosition;

		/// <summary>螢幕中心位置</summary>
		public static Vector3 centerPos => new Vector3(Screen.width / 2f, Screen.height / 2f);

		/// <returns>二維座標軸(WASD鍵)方向</returns>
		public static Vector2 axisDir => new Vector2(
		Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

		/// <returns>指定圓心及半徑的圓內隨機點</returns>
		public static Vector2 RandomInsideCircle(Vector2 center, float radius = 1f){
			return UnityEngine.Random.insideUnitCircle * Mathf.Abs(radius) + center;
		}

		/// <returns>隨機二維正規化向量</returns>
		public static Vector2 RandomDir2D => RandomInsideCircle(Vector2.zero).normalized;

		/// <summary>將以度為單位的角轉換為二維正規化向量</summary>
		public static Vector2 AngleToVector2(this float angle){
			float degree = angle % 360f;
			float radian = degree / 180f * Mathf.PI;
			float x = Mathf.Cos(radian);
			float y = Mathf.Sin(radian);
			return new Vector2(x, y).normalized;
		}

		/// <summary>將二維向量轉換為以度為單位的角</summary>
		public static float Vector2ToAngle(this Vector2 vector2){
			Vector2 dir = vector2.normalized;
			// float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			float angle = Vector2.SignedAngle(Vector2.right, dir);
			return angle < 0 ? angle + 360f : angle;
		}

		/// <summary>正規化角度到 0° 和 360° 之間或 -180° 和 +180° 之間</summary>
		/// <param name="toFullCircle">正規化為 0-360°, 否則為 +/-180°</param>
		public static float NormalizeAngle(this float angle, bool toFullCircle = false){
			if(toFullCircle){
				angle %= 360f;
				return Math.Sign(angle) < 0f ? angle + 360f : angle;
			}
			else{
				return angle % 180f;
			}
		}

		/// <summary>將螢幕位置轉換為世界座標</summary>
		/// <remarks>
		/// 傳回位於遊戲層級視圖最頂層的世界座標點<br/>
		/// 若相機未指定則使用預設值 Camera.main<br/>
		/// 螢幕的左下角座標為零 右上角座標為相機寬度和高度<br/>
		/// 當 ignoreDepth 啟用時傳回座標的 z 值為零<br/>
		/// </remarks>
		public static Vector3 ScreenToWorld(this Vector3 pos,
		Camera cam, bool ignoreDepth = false){
			cam = cam == default(Camera) ? Camera.main : cam;
			Vector3 point = cam.ScreenToWorldPoint(pos);
			point.z = ignoreDepth ? 0f : point.z;
			return point;
		}

		/// <summary>將世界位置轉換為螢幕座標</summary>
		/// <remarks>
		/// 傳回以像素為單位的螢幕座標點<br/>
		/// 若相機未指定則使用預設值 Camera.main<br/>
		/// 螢幕的左下角座標為零 右上角座標為相機寬度和高度<br/>
		/// 當 ignoreDepth 禁用時傳回座標的 z 值為世界位置和相機的距離<br/>
		/// 當 ignoreDepth 啟用時傳回座標的 z 值為零<br/>
		/// </remarks>
		public static Vector3 WorldToScreen(this Vector3 pos,
		Camera cam, bool ignoreDepth = false){
			cam = cam == default(Camera) ? Camera.main : cam;
			Vector3 point = cam.WorldToScreenPoint(pos);
			point.z = ignoreDepth ? 0f : point.z;
			return point;
		}

		/// <summary>將矩形變換位置轉換為世界座標</summary>
		/// <remarks>
		/// 傳回位於遊戲層級視圖最頂層的世界座標點<br/>
		/// 若相機未指定則使用預設值 Camera.main<br/>
		/// 當 ignoreDepth 啟用時傳回座標的 z 值為零<br/>
		/// </remarks>
		public static Vector3 UIToWorld(this Vector3 pos,
		RectTransform canvas, Camera cam, bool ignoreDepth = false){
			float w = canvas.rect.width / 2f;
			float h = canvas.rect.height / 2f;
			Vector3 view = new Vector3(((pos.x / w) + 1f) / 2f, ((pos.y / h) + 1f) / 2f, pos.z);
			cam = cam == default(Camera) ? Camera.main : cam;
			Vector3 point = cam.ViewportToWorldPoint(view);
			point.z = ignoreDepth ? 0f : point.z;
			return point;
		}

		/// <summary>將世界位置轉換為矩形變換座標</summary>
		/// <remarks>
		/// 傳回矩形變換座標點<br/>
		/// 若相機未指定則使用預設值 Camera.main<br/>
		/// </remarks>
		public static Vector2 WorldToUI(this Vector3 pos,
		RectTransform canvas, Camera cam){
			cam = cam == default(Camera) ? Camera.main : cam;
			Vector2 screen = cam.WorldToViewportPoint(pos);
			Vector2 view = (screen - canvas.pivot) * 2f;
			float w = canvas.rect.width / 2f;
			float h = canvas.rect.height / 2f;
			return new Vector2(view.x * w, view.y * h);
		}

		/// <summary>判斷世界位置是否在相機視角範圍內</summary>
		public static bool IsVisableInCamera(this Vector3 pos,
		Camera cam, bool ignoreBack = false){
			cam = cam == default(Camera) ? Camera.main : cam;
			Vector3 point = cam.WorldToViewportPoint(pos);
			if(point.z < 0f && !ignoreBack) return false;
			if(point.x < 0f || point.x > 1f) return false;
			if(point.y < 0f || point.y > 1f) return false;
			return true;
		}

		/// <summary>建立指定方向的二維旋轉</summary>
		/// <param name="start">方向起點</param>
		/// <param name="end">方向終點</param>
		/// <param name="offset">角度偏移</param>
		public static Quaternion LookRotation2D(Vector2 start, Vector2 end, float offset = 0f){
			Vector2 dir = (end - start).normalized;
			float angle = dir.Vector2ToAngle();
			angle += offset;
			return Quaternion.Euler(0f, 0f, angle);
		}

		/// <summary>將二維向量在特定角度內隨機偏移</summary>
		public static Vector2 RandomOffset2D(this Vector2 vector2, float angle){
			angle = Mathf.Abs(angle);
			angle = angle / 2f;
			angle = UnityEngine.Random.Range(-angle, +angle);
			angle = Vector2ToAngle(vector2) + angle;
			return AngleToVector2(angle) * vector2.magnitude;
		}

		/// <summary>計算二維碰撞反彈方向</summary>
		public static Vector2 BounceDirection(Vector2 origDir, Collision2D collision){
			Vector2 dir = origDir.normalized;
			Vector2 nor = collision.contacts[0].normal.normalized;
			return Vector2.Reflect(dir, nor).normalized;
		}

		/// <summary>在指定的二維軸向上翻轉角度</summary>
		public static float FlipAngle(this float angle, Axis axis){
			if(axis == Axis.X) return 180f - angle;
			else if(axis == Axis.Y) return 0 - angle;
			else return angle;
		}

		/// <summary>將角度交叉映射到X軸正向</summary>
		/// <remarks>
		/// 第一象限 -> 第一象限<br/>
		/// 第二象限 -> 第四象限<br/>
		/// 第三象限 -> 第一象限<br/>
		/// 第四象限 -> 第四象限<br/>
		/// </remarks>
		public static float CrossingToPositiveX(this float angle){
			angle = NormalizeAngle(angle, true);
			if(angle > 90f && angle < 270f){
				angle += 180f;
			}
			return angle;
		}

		#region obsolete

		/// <summary>根據符號翻轉此變換的縮放軸向</summary>
		/// <param name="reverse">是否反相判定符號</param>
		// [Obsolete("using inline syntax")]
		public static void FlipScaleAxis(this Transform trans,
		Axis axis, float sign, bool reverse = false){
			sign = Mathf.Sign(sign);
			if(reverse){
				sign = -sign;
			}
			float x = trans.localScale.x;
			float y = trans.localScale.y;
			float z = trans.localScale.z;
			x = axis == Axis.X ? Mathf.Abs(x) * sign : x;
			y = axis == Axis.Y ? Mathf.Abs(y) * sign : y;
			z = axis == Axis.Z ? Mathf.Abs(z) * sign : z;
			trans.localScale = new Vector3(x, y, z);
		}

		/// <summary>旋轉此變換使之朝向游標</summary>
		/// <param name="offset">角度偏移</param>
		// [Obsolete("using inline syntax")]
		public static void LookAtCursor2D(
		this Transform trans, Camera cam, float offset = 0f){
			cam = cam == default(Camera) ? Camera.main : cam;
			Vector2 pos = cam.WorldToScreenPoint(trans.position);
			trans.rotation = LookRotation2D(pos, Input.mousePosition, offset);
		}

		/// <summary>旋轉並翻轉此變換使之朝向游標</summary>
		/// <param name="offset">旋轉角度偏移</param>
		/// <param name="reverse">反向判定翻轉</param>
		// [Obsolete("using inline syntax")]
		public static void LookAtCursorFlip2D(this Transform trans,
		Camera cam, float offset = 0f, bool reverse = false){
			cam = cam == default(Camera) ? Camera.main : cam;
			Vector2 dir = Input.mousePosition - cam.WorldToScreenPoint(trans.position);
			float angle = dir.Vector2ToAngle();
			angle = CrossingToPositiveX(angle);
			angle += offset;
			trans.rotation = Quaternion.Euler(0f, 0f, angle);
			float x = cursorPos.ScreenToWorld(cam).x - trans.position.x;
			trans.FlipScaleAxis(Axis.X, x, reverse);
		}

		#endregion
	}
}
