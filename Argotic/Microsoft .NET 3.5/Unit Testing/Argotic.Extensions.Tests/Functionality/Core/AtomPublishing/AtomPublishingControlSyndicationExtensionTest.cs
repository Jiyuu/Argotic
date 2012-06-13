﻿using Argotic.Extensions.Core;
using Argotic.Syndication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Globalization;

namespace Argotic.Extensions.Tests
{
	/// <summary>
	///This is a test class for AtomPublishingControlSyndicationExtensionTest and is intended
	///to contain all AtomPublishingControlSyndicationExtensionTest Unit Tests
	///</summary>
	[TestClass()]
	public class AtomPublishingControlSyndicationExtensionTest
	{

		const string namespc = @"xmlns:app=""http://www.w3.org/2007/app""";

		private const string nycText = "<control xml:base=\"http://www.example.com/control.html\" xml:lang=\"en-US\" xmlns=\"http://www.w3.org/2007/app\">\r\n"+
										"  <draft>yes</draft>\r\n" +
										"</control>";

		private const string strExtXml = "<app:control xml:base=\"http://www.example.com/control.html\" xml:lang=\"en-US\"><app:draft>yes</app:draft></app:control>";

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///A test for AtomPublishingControlSyndicationExtension Constructor
		///</summary>
		[TestMethod()]
		public void AtomPublishingControlSyndicationExtensionConstructorTest()
		{
			AtomPublishingControlSyndicationExtension target = new AtomPublishingControlSyndicationExtension();
			Assert.IsNotNull(target);
			Assert.IsInstanceOfType(target, typeof(AtomPublishingControlSyndicationExtension));
		}

		/// <summary>
		///A test for CompareTo
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_CompareToTest()
		{
			AtomPublishingControlSyndicationExtension target = CreateExtension1();
			object obj = CreateExtension1();
			int expected = 0; 
			int actual;
			actual = target.CompareTo(obj);
			Assert.AreEqual(expected, actual);
		}


		/// <summary>
		///A test for Equals
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_EqualsTest()
		{
			AtomPublishingControlSyndicationExtension target = CreateExtension1();
			object obj = CreateExtension1();
			bool expected = true;
			bool actual;
			actual = target.Equals(obj);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for GetHashCode
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_GetHashCodeTest()
		{
			AtomPublishingControlSyndicationExtension target = CreateExtension1();
			int expected = -441830618;
			int actual;
			actual = target.GetHashCode();
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Load
		///</summary>
		[TestMethod, Ignore]
		public void AtomPublishingControl_LoadTest()
		{
			AtomPublishingControlSyndicationExtension target = new AtomPublishingControlSyndicationExtension(); // TODO: Initialize to an appropriate value
			var nt = new NameTable();
			var ns = new XmlNamespaceManager(nt);
			 var xpc = new XmlParserContext(nt, ns, "US-en",XmlSpace.Default);
			 var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

			using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, xpc)	)
			{
#if false
				//var document  = new XPathDocument(reader);
				//var nav = document.CreateNavigator();
				//nav.Select("//item");
				do
				{
					if (!reader.Read())
						break;
				} while (reader.NodeType != XmlNodeType.EndElement || reader.Name != "webMaster");

				
				bool expected = true;
				bool actual;
				actual = target.Load(reader);
				Assert.AreEqual(expected, actual);
#else
				RssFeed feed = new RssFeed();
				feed.Load(reader);
#endif
			}
		}

	[TestMethod]
		public void AtomPublishingControl_CreateXmlTest()
	  {
		  var itunes = CreateExtension1();

		  var actual = ExtensionTestUtil.AddExtensionToXml(itunes);
		  string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
		  Assert.AreEqual(expected, actual);
	  }


		[TestMethod, Ignore]
	public void AtomPublishingControl_FullTest()
		{
			var strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

			 using (XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null))
			 {
				 RssFeed feed = new RssFeed();
				 feed.Load(reader);

				 //				 Assert.IsTrue(feed.Channel.HasExtensions);
				 //				 Assert.IsInstanceOfType(feed.Channel.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) as AtomPublishingControlSyndicationExtension,
				 //						 typeof(AtomPublishingControlSyndicationExtension));

				 Assert.AreEqual(1, feed.Channel.Items.Count());
				 var item = feed.Channel.Items.Single();
				 var ext = item.HasExtensions;
				 Assert.IsTrue(item.HasExtensions);
				 var itemExtension = item.FindExtension<AtomPublishingControlSyndicationExtension>();
				 Assert.IsNotNull(itemExtension);
				 Assert.IsInstanceOfType(item.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) as AtomPublishingControlSyndicationExtension,
				  typeof(AtomPublishingControlSyndicationExtension));

			 }
		}

		/// <summary>
		///A test for MatchByType
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_MatchByTypeTest()
		{
			ISyndicationExtension extension = CreateExtension1();
			bool expected = true;
			bool actual;
			actual = AtomPublishingControlSyndicationExtension.MatchByType(extension);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for ToString
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_ToStringTest()
		{
			AtomPublishingControlSyndicationExtension target = CreateExtension1();
			string expected = nycText;
			string actual;
			actual = target.ToString();
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for WriteTo
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_WriteToTest()
		{
			using(var sw = new StringWriter())
			using (XmlWriter writer = new XmlTextWriter(sw))
			{

				var target = CreateExtension1();
				target.WriteTo(writer);
				var output = sw.ToString();
				Assert.AreEqual(nycText.Replace(Environment.NewLine+"  ", "").Replace(Environment.NewLine, ""), output.Replace(Environment.NewLine, ""));
			}
		}

		/// <summary>
		///A test for op_Equality
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_op_EqualityTest_Failure()
		{
			AtomPublishingControlSyndicationExtension first = CreateExtension1();
			AtomPublishingControlSyndicationExtension second = CreateExtension2();
			bool expected = false; 
			bool actual;
			actual = (first == second);
			Assert.AreEqual(expected, actual);
		}

		public void AtomPublishingControl_op_EqualityTest_Success()
		{
			AtomPublishingControlSyndicationExtension first = CreateExtension1();
			AtomPublishingControlSyndicationExtension second = CreateExtension1();
			bool expected = true;
			bool actual;
			actual = (first == second);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for op_GreaterThan
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_op_GreaterThanTest()
		{
			AtomPublishingControlSyndicationExtension first = CreateExtension1();
			AtomPublishingControlSyndicationExtension second = CreateExtension2();
			bool expected = false; 
			bool actual = false;
			actual = (first > second);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for op_Inequality
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_op_InequalityTest()
		{
			AtomPublishingControlSyndicationExtension first = CreateExtension1();
			AtomPublishingControlSyndicationExtension second = CreateExtension2();
			bool expected = true;
			bool actual = (first != second);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for op_LessThan
		///</summary>
		[TestMethod()]
		public void AtomPublishingControl_op_LessThanTest()
		{
			AtomPublishingControlSyndicationExtension first = CreateExtension1();
			AtomPublishingControlSyndicationExtension second = CreateExtension2();
			bool expected = true; 
			bool actual;
			actual = (first < second);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Context
		///</summary>
		[TestMethod(), Ignore]
		public void AtomPublishingControl_ContextTest()
		{
			AtomPublishingControlSyndicationExtension target = CreateExtension1();
			AtomPublishingControlSyndicationExtensionContext expected =CreateContext1();
			AtomPublishingControlSyndicationExtensionContext actual;
//			target.Context = expected;
			actual = target.Context;
			var b = actual.Equals(expected);
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		private AtomPublishingControlSyndicationExtension CreateExtension1()
		{
			var nyc = new AtomPublishingControlSyndicationExtension();

			nyc.Context.BaseUri = new Uri("http://www.example.com/control.html");
			nyc.Context.IsDraft = true;
			nyc.Context.Language = new CultureInfo("en-US");
//			nyc.Context.AddExtension()

			return nyc;
		}
		private AtomPublishingControlSyndicationExtension CreateExtension2()
		{
			var nyc = new AtomPublishingControlSyndicationExtension();
			nyc.Context.BaseUri = new Uri("http://www.example.net/control.html");
			nyc.Context.IsDraft = false;
			nyc.Context.Language = new CultureInfo("fr-CA");
			return nyc;
		}

		public static AtomPublishingControlSyndicationExtensionContext CreateContext1()
		{
			var nyc = new AtomPublishingControlSyndicationExtensionContext();
			//nyc.Latitude = 40;
			//nyc.Longitude = -74;
			return nyc;
		}

	}
}
