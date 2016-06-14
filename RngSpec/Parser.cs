using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;

namespace RngSpec
{
	public abstract class Element
	{
		/// <summary>
		/// Converts from XML case (<c>the-quick-brown</c>) to Pascal case (<c>TheQuickBrown</c>).
		/// Additionally prepends with an underscore if the first character is a digit, and removes
		/// non letter/digit characters.
		/// </summary>
		/// <param name="xmlStr">An XML case string</param>
		/// <returns>The string as a valid C# identifier</returns>
		public static string ConvertCase(string xmlStr)
		{
			var sb = new StringBuilder();
			var cap = true;
			if (char.IsDigit(xmlStr[0]))
				sb.Append('_');
			foreach (var c in xmlStr)
			{
				if (char.IsLetter(c))
				{
					if (cap)
					{
						sb.Append(char.ToUpperInvariant(c));
						cap = false;
					}
					else
						sb.Append(c);
				}
				else
				{
					if (char.IsDigit(c))
						sb.Append(c);
					cap = true;
				}
			}
			return sb.ToString();
		}

		private readonly Dictionary<string, int> candidateNames = new Dictionary<string, int>();

		public void AddCandidateName(string name)
		{
			if(candidateNames.ContainsKey(name))
				candidateNames[name]++;
			else
				candidateNames.Add(name, 1);
		}

		public string GetName()
		{
			return candidateNames.Max((a, b) => a.Value.CompareTo(b.Value)).Key;
		}
	}

	public class XsElement : Element
	{
		public XElement XElement { get; set; }

		public XsElement(XElement element)
		{
			AddCandidateName(element.Name.LocalName);
			XElement = element;
		}
	}

	public abstract class DataTypeElement : Element
	{
		public abstract Type Type { get; }
	}

	public class PrimitiveTypeElement : DataTypeElement
	{
		public override Type Type { get; }

		public PrimitiveTypeElement(Type type)
		{
			Type = type;
		}
	}

	public class EnumElement : DataTypeElement
	{
		public override Type Type => type;

		private readonly XElement choice;

		private Type type;

		public void Create(ModuleBuilder module)
		{

			var enumType = module.DefineEnum(ConvertCase(GetName()), TypeAttributes.Public, typeof(int));
			int i = 0;
			foreach (var v in choice.Elements(XName.Get("value")))
				enumType.DefineLiteral(ConvertCase(v.Value), i++);
			type = enumType.CreateType();
		}

		public EnumElement(XElement choice)
		{
			this.choice = choice;
		}
	}

	public class ElementObject : Element
	{
		
	}

	public class Attribute : Element
	{


		public void Create(TypeBuilder eType)
		{
			//var property = eType.DefineProperty(ConvertCase(GetName()), PropertyAttributes.None, )
		}
	}
	
	public class Parser
	{
		private XElement grammar;
		private XElement start;
		private Dictionary<string, Element> defines = new Dictionary<string, Element>();

		private readonly Dictionary<string, string> nsMap = new Dictionary<string, string>();

		public Parser(string path)
		{
			grammar = XDocument.Load(path).Element(XName.Get("grammar"));
			if(grammar == null) throw new Exception("No grammar element");
			foreach (var a in grammar.Attributes().Where(a => a.IsNamespaceDeclaration))
				nsMap[a.Name.LocalName] = a.Value;
			start = grammar.Element(XName.Get("start"));
			if(start == null) throw new Exception("No start element");
			foreach (var d in grammar.Elements(XName.Get("define")))
				defines[d.Attribute(XName.Get("name")).Value] = new XsElement(d);
		}

		void Expand(ref Element e)
		{
			
		}
	}
}
