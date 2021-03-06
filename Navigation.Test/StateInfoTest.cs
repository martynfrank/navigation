﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
#if NET40Plus
using System.Web.Routing;
#endif
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Navigation.Test
{
	[TestClass]
	public class StateInfoTest
	{
		public StateInfoTest()
		{
		}

		private TestContext testContextInstance;

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

		[AssemblyInitialize]
		public static void SetCustomHandler(TestContext context)
		{
#if NET40Plus
			FluentStateInfoConfig.Register(StateInfoConfig.Fluent);
#endif
			StateInfoConfig.Dialogs[6].States[0].StateHandler = new CustomStateHandler();
			StateInfoConfig.Dialogs[6].States[1].StateHandler = new CustomStateHandler();
			StateController.Navigate("d0");
		}

		[TestMethod]
		public void DialogTest()
		{
			Assert.AreEqual(9, StateInfoConfig.Dialogs.Count);
			int i = 0;
			foreach (Dialog dialog in StateInfoConfig.Dialogs)
			{
				Assert.AreEqual("d" + i, dialog.Key);
				Assert.AreEqual(dialog.Key, dialog.Title);
				Assert.AreEqual(i, dialog.Index);
				Assert.AreEqual("s0", dialog.Initial.Title);
				Assert.AreEqual("d" + i, dialog.Path);
				i++;
			}
		}

		[TestMethod]
		public void StateTest()
		{
			string page = "~/{0}/{1}.aspx";
			string mobilePage = "~/mobile/{0}/{1}.aspx";
			int i = 0;
			foreach (Dialog dialog in StateInfoConfig.Dialogs)
			{
				if (dialog.Index < 6)
					Assert.AreEqual(5 + dialog.Index % 3, dialog.States.Count);
				i = 0;
				foreach (State state in StateInfoConfig.Dialogs[dialog.Key].States)
				{
					Assert.AreEqual("s" + i, state.Key);
					Assert.AreEqual(state.Key, state.Title);
					Assert.AreEqual(i, state.Index);
					if (dialog.Index < 6)
						Assert.AreEqual(string.Format(page, dialog.Key, state.Key), state.Page);
					if (i == 1 && dialog.Index < 3)
						Assert.AreEqual(string.Format(mobilePage, dialog.Key, state.Key), state.MobilePage);
					i++;
				}
			}
		}

		[TestMethod]
		public void TransitionTest()
		{
			int i = 0;
			foreach (Dialog dialog in StateInfoConfig.Dialogs)
			{
				foreach (State state in StateInfoConfig.Dialogs[dialog.Key].States)
				{
					i = 0;
					if (dialog.Index == 0)
						Assert.AreEqual(4 - state.Index, state.Transitions.Count);
					if (dialog.Index == 1)
					{
						if (state.Index != 5)
							Assert.AreEqual(1, state.Transitions.Count);
						else
							Assert.AreEqual(5, state.Transitions.Count);
					}
					if (dialog.Index == 2)
						Assert.AreEqual(1, state.Transitions.Count);
					foreach (Transition transition in dialog.States[state.Key].Transitions)
					{
						Assert.AreEqual("t" + i, transition.Key);
						Assert.AreEqual(i, transition.Index);
						i++;
					}
				}
			}
		}

		[TestMethod]
		public void DialogInitialTest()
		{
			foreach (Dialog dialog in StateInfoConfig.Dialogs)
			{
				Assert.AreEqual(dialog.States[0], dialog.Initial);
			}
		}

		[TestMethod]
		public void DialogAttributesTest()
		{
#if NET40Plus
			Assert.AreEqual(2, StateInfoConfig.Dialogs[6].Attributes.Count);
			Assert.AreEqual("true", StateInfoConfig.Dialogs[6].Attributes["other"]);
			Assert.AreEqual(" d6", StateInfoConfig.Dialogs[6].Attributes[1]);
#else
			Assert.AreEqual(5, StateInfoConfig.Dialogs[6].Attributes.Count);
			Assert.AreEqual("d6", StateInfoConfig.Dialogs[6].Attributes["key"]);
			Assert.AreEqual("s0", StateInfoConfig.Dialogs[6].Attributes[1]);
			Assert.AreEqual("d6", StateInfoConfig.Dialogs[6].Attributes["title"]);
			Assert.AreEqual(" d6", StateInfoConfig.Dialogs[6].Attributes["path"]);
#endif
		}

		[TestMethod]
		public void StateParentTest()
		{
			foreach (Dialog dialog in (IEnumerable<Dialog>) StateInfoConfig.Dialogs)
			{
				foreach (State state in StateInfoConfig.Dialogs[dialog.Index].States)
				{
					Assert.AreEqual(dialog, state.Parent);
				}
			}
		}

		[TestMethod]
		public void TransitionParentTest()
		{
			foreach (Dialog dialog in StateInfoConfig.Dialogs)
			{
				foreach (State state in StateInfoConfig.Dialogs[dialog.Index].States)
				{
					foreach (Transition transition in StateInfoConfig.Dialogs[dialog.Index].States[state.Key].Transitions)
					{
						Assert.AreEqual(state, transition.Parent);
					}
				}
			}
		}

		[TestMethod]
		public void TransitionToTest()
		{
			foreach (Dialog dialog in StateInfoConfig.Dialogs)
			{
				foreach (State state in dialog.States)
				{
					foreach (Transition transition in state.Transitions)
					{
						Assert.AreEqual(dialog.States[transition.To.Key], transition.To);
					}
				}
			}
		}

		[TestMethod]
		public void MasterThemeTest()
		{
			Assert.AreEqual(string.Empty, StateInfoConfig.Dialogs[0].States[0].Theme);
			Assert.AreEqual(0, StateInfoConfig.Dialogs[0].States[0].Masters.Count);
			Assert.AreEqual("test", StateInfoConfig.Dialogs[1].States[0].Theme);
			Assert.AreEqual("test1", StateInfoConfig.Dialogs[1].States[0].Masters[0]);
			Assert.AreEqual("test2", StateInfoConfig.Dialogs[1].States[0].Masters[1]);
			Assert.AreEqual("", StateInfoConfig.Dialogs[1].States[1].Masters[0]);
			Assert.AreEqual("", StateInfoConfig.Dialogs[1].States[1].Masters[1]);
			Assert.AreEqual("!@", StateInfoConfig.Dialogs[1].States[2].Masters[0]);
		}

		[TestMethod]
		public void MobileMasterThemeTest()
		{
			Assert.AreEqual(string.Empty, StateInfoConfig.Dialogs[0].States[0].MobileTheme);
			Assert.AreEqual(0, StateInfoConfig.Dialogs[0].States[0].MobileMasters.Count);
			Assert.AreEqual("test", StateInfoConfig.Dialogs[1].States[3].MobileTheme);
			Assert.AreEqual("test1", StateInfoConfig.Dialogs[1].States[3].MobileMasters[0]);
			Assert.AreEqual("test2", StateInfoConfig.Dialogs[1].States[3].MobileMasters[1]);
			Assert.AreEqual("", StateInfoConfig.Dialogs[1].States[4].MobileMasters[0]);
			Assert.AreEqual("", StateInfoConfig.Dialogs[1].States[4].MobileMasters[1]);
			Assert.AreEqual("!@", StateInfoConfig.Dialogs[1].States[5].MobileMasters[0]);
		}

		[TestMethod]
		public void TrackCrumbTrailTest()
		{
			Assert.AreEqual(false, StateInfoConfig.Dialogs[2].States[0].TrackCrumbTrail);
			Assert.AreEqual(true, StateInfoConfig.Dialogs[2].States[1].TrackCrumbTrail);
			Assert.AreEqual(false, StateInfoConfig.Dialogs[2].States[2].TrackCrumbTrail);
			Assert.AreEqual(true, StateInfoConfig.Dialogs[2].States[3].TrackCrumbTrail);
			Assert.AreEqual(false, StateInfoConfig.Dialogs[2].States[4].TrackCrumbTrail);
			Assert.AreEqual(true, StateInfoConfig.Dialogs[2].States[5].TrackCrumbTrail);
			Assert.AreEqual(true, StateInfoConfig.Dialogs[2].States[6].TrackCrumbTrail);
		}

		[TestMethod]
		public void DefaultsTest()
		{
			Assert.IsTrue(StateInfoConfig.Dialogs[0].States[0].Defaults.Count == 0);
			Assert.AreEqual("Hello", StateInfoConfig.Dialogs[0].States[1].Defaults["string"]);
			Assert.AreEqual(true, StateInfoConfig.Dialogs[0].States[1].Defaults["_bool"]);
			Assert.AreEqual(0, StateInfoConfig.Dialogs[0].States[1].Defaults["_int"]);
			Assert.AreEqual((short) 1, StateInfoConfig.Dialogs[0].States[1].Defaults["short"]);
			Assert.AreEqual(2L, StateInfoConfig.Dialogs[0].States[1].Defaults["long"]);
			Assert.AreEqual(3F, StateInfoConfig.Dialogs[0].States[1].Defaults["float"]);
			Assert.AreEqual(null, StateInfoConfig.Dialogs[0].States[1].Defaults["another"]);
			Assert.IsTrue(StateInfoConfig.Dialogs[0].States[1].Defaults.Count == 6);
			Assert.IsTrue(StateInfoConfig.Dialogs[0].States[2].Defaults.Count == 6);
			Assert.AreEqual(4D, StateInfoConfig.Dialogs[0].States[2].Defaults["double"]);
			Assert.AreEqual(5m, StateInfoConfig.Dialogs[0].States[2].Defaults["decimal"]);
			Assert.AreEqual(new DateTime(1990, 3, 1, 12, 35, 47), StateInfoConfig.Dialogs[0].States[2].Defaults["DateTime"]);
			Assert.AreEqual((byte)6, StateInfoConfig.Dialogs[0].States[2].Defaults["byte"]);
			Assert.AreEqual('7', StateInfoConfig.Dialogs[0].States[2].Defaults["char"]);
		}

		[TestMethod]
		public void DefaultTypesTest()
		{
			Assert.IsTrue(StateInfoConfig.Dialogs[0].States[0].DefaultTypes.Count == 0);
			Assert.AreEqual(typeof(string), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["string"]);
			Assert.AreEqual(typeof(bool), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["bool"]);
			Assert.AreEqual(typeof(int), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["int"]);
			Assert.AreEqual(typeof(short), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["short"]);
			Assert.AreEqual(typeof(long), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["long"]);
			Assert.AreEqual(typeof(float), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["float"]);
			Assert.AreEqual(typeof(double), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["double"]);
			Assert.AreEqual(typeof(decimal), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["decimal"]);
			Assert.AreEqual(typeof(DateTime), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["DateTime"]);
			Assert.AreEqual(typeof(TimeSpan), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["TimeSpan"]);
			Assert.AreEqual(typeof(byte), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["byte"]);
			Assert.AreEqual(typeof(char), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["char"]);
			Assert.AreEqual(typeof(Guid), StateInfoConfig.Dialogs[0].States[4].DefaultTypes["Guid"]);
		}

		[TestMethod]
		public void DerivedTest()
		{
			Assert.IsTrue(StateInfoConfig.Dialogs[0].States[0].Derived.Count == 0);
			Assert.AreEqual("st1", StateInfoConfig.Dialogs[0].States[1].Derived[0]);
			Assert.AreEqual("bool", StateInfoConfig.Dialogs[0].States[1].Derived[1]);
			Assert.AreEqual("long", StateInfoConfig.Dialogs[0].States[1].Derived[2]);
		}

		[TestMethod]
		public void AttributesTest()
		{
#if NET40Plus
			Assert.AreEqual(2, StateInfoConfig.Dialogs[6].States[0].Attributes.Count);
			Assert.AreEqual("~/d6/s0.aspx", StateInfoConfig.Dialogs[6].States[0].Attributes[0]);
			Assert.AreEqual(2, StateInfoConfig.Dialogs[6].States[1].Attributes.Count);
#else
			Assert.AreEqual(4, StateInfoConfig.Dialogs[6].States[0].Attributes.Count);
			Assert.AreEqual("s0", StateInfoConfig.Dialogs[6].States[0].Attributes["key"]);
			Assert.AreEqual("s0", StateInfoConfig.Dialogs[6].States[0].Attributes["title"]);
			Assert.AreEqual("~/d6/s0.aspx", StateInfoConfig.Dialogs[6].States[0].Attributes[2]);
			Assert.AreEqual(4, StateInfoConfig.Dialogs[6].States[1].Attributes.Count);
			Assert.AreEqual("s1", StateInfoConfig.Dialogs[6].States[1].Attributes["key"]);
			Assert.AreEqual("s1", StateInfoConfig.Dialogs[6].States[1].Attributes["title"]);
#endif
			Assert.AreEqual("~/d6/s1.aspx", StateInfoConfig.Dialogs[6].States[1].Attributes["handler"]);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void StateHandlerNullTest()
		{
			StateInfoConfig.Dialogs[0].States[0].StateHandler = null;
		}

#if NET40Plus
		[TestMethod]
		public void RouteTest()
		{
			Assert.AreEqual(string.Empty, StateInfoConfig.Dialogs[0].States[0].Route);
			Assert.AreEqual(string.Empty, StateInfoConfig.Dialogs[0].States[0].MobileRoute);
			Assert.AreEqual("d3s0", StateInfoConfig.Dialogs[3].States[0].Route);
			Assert.AreEqual("m/d3s0", StateInfoConfig.Dialogs[3].States[0].MobileRoute);
			Assert.AreEqual("d3s1/{string}/{short}", StateInfoConfig.Dialogs[3].States[1].Route);
			Assert.AreEqual(string.Empty, StateInfoConfig.Dialogs[3].States[1].MobileRoute);
			Assert.AreEqual(" d4s5", StateInfoConfig.Dialogs[4].States[5].Route);
			Assert.AreEqual("md4s5 ", StateInfoConfig.Dialogs[4].States[5].MobileRoute);
		}

		[TestMethod]
		public void PageRouteMapTest()
		{
			StateController.Navigate("d0");
			Assert.IsNotNull(RouteTable.Routes["3-0"]);
			Assert.IsNotNull(RouteTable.Routes["Mobile3-0"]);
			Assert.IsNotNull(RouteTable.Routes["3-1"]);
			Assert.IsNull(RouteTable.Routes["Mobile3-1"]);
		}

		[TestMethod]
		public void PageRouteDefaultsTest()
		{
			StateController.Navigate("d0");
			Route route = RouteTable.Routes["3-0"] as Route;
			Assert.AreEqual(0, route.Defaults.Count);
			route = RouteTable.Routes["3-1"] as Route;
			Assert.AreEqual(2, route.Defaults.Count);
			route = RouteTable.Routes["3-2"] as Route;
			Assert.AreEqual(2, route.Defaults.Count);
		}
#endif

#if NET45Plus
		[TestMethod]
		public void PageRouteHandlerTest()
		{
			StateController.Navigate("d0");
			StateRouteHandler handler = ((Route)RouteTable.Routes["3-0"]).RouteHandler as StateRouteHandler;
			Assert.IsNull(handler);
			handler = ((Route)RouteTable.Routes["3-1"]).RouteHandler as StateRouteHandler;
			Assert.IsNotNull(handler);
			Assert.AreEqual(StateInfoConfig.Dialogs[3].States[1], handler.State);
			handler = ((Route)RouteTable.Routes["3-2"]).RouteHandler as StateRouteHandler;
			Assert.IsNull(handler);
			handler = ((Route)RouteTable.Routes["3-3"]).RouteHandler as StateRouteHandler;
			Assert.IsNull(handler);
			handler = ((Route)RouteTable.Routes["3-4"]).RouteHandler as StateRouteHandler;
			Assert.IsNull(handler);
		}
#endif

		[TestMethod]
		public void StateInfoCopyToTest()
		{
			Dialog[] dialogArr = new Dialog[9];
			State[] stateArr = new State[5];
			Transition[] transitionArr = new Transition[5];
			StateInfoConfig.Dialogs.CopyTo(dialogArr, 0);
			StateInfoConfig.Dialogs[0].States.CopyTo(stateArr, 0);
			StateInfoConfig.Dialogs[0].States[0].Transitions.CopyTo(transitionArr, 1);
			Assert.AreEqual(dialogArr[0], StateInfoConfig.Dialogs[0]);
			Assert.AreEqual(dialogArr[1], StateInfoConfig.Dialogs[1]);
			Assert.AreEqual(dialogArr[2], StateInfoConfig.Dialogs[2]);
			Assert.AreEqual(stateArr[0], StateInfoConfig.Dialogs[0].States[0]);
			Assert.AreEqual(stateArr[1], StateInfoConfig.Dialogs[0].States[1]);
			Assert.AreEqual(stateArr[2], StateInfoConfig.Dialogs[0].States[2]);
			Assert.AreEqual(stateArr[3], StateInfoConfig.Dialogs[0].States[3]);
			Assert.AreEqual(stateArr[4], StateInfoConfig.Dialogs[0].States[4]);
			Assert.AreEqual(transitionArr[1], StateInfoConfig.Dialogs[0].States[0].Transitions[0]);
			Assert.AreEqual(transitionArr[2], StateInfoConfig.Dialogs[0].States[0].Transitions[1]);
			Assert.AreEqual(transitionArr[3], StateInfoConfig.Dialogs[0].States[0].Transitions[2]);
			Assert.AreEqual(transitionArr[4], StateInfoConfig.Dialogs[0].States[0].Transitions[3]);
		}

		[TestMethod]
		public void StateInfoSerializeTest()
		{
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();
			formatter.Serialize(stream, StateInfoConfig.Dialogs);
			stream.Seek(0, SeekOrigin.Begin);
			StateInfoCollection<Dialog> dialogs = (StateInfoCollection<Dialog>)formatter.Deserialize(stream);
			Assert.AreEqual(dialogs.Count, StateInfoConfig.Dialogs.Count);
			Assert.AreEqual(dialogs[0].States.Count, StateInfoConfig.Dialogs[0].States.Count);
			Assert.AreEqual(dialogs[0].States[0].Transitions.Count, StateInfoConfig.Dialogs[0].States[0].Transitions.Count);
			Assert.AreEqual(dialogs[0].States[0].Parent, dialogs[0]);
			Assert.AreEqual(dialogs[0].States[0].Transitions[0].Parent, dialogs[0].States[0]);
			Assert.IsTrue(dialogs[0].States[1].Defaults.Count == 6);
			Assert.IsTrue(dialogs[0].States[2].Defaults.Count == 6);
			Assert.IsTrue(dialogs[0].States[1].Derived.Count == 3);
			Assert.IsTrue(dialogs[0].States[2].Derived.Count == 2);
#if NET40Plus
			Assert.IsTrue(dialogs[0].States[1].DefaultTypes.Count == 6);
			Assert.IsTrue(dialogs[0].States[2].DefaultTypes.Count == 7);
			Assert.IsTrue(dialogs[6].Attributes.Count == 2);
#else
			Assert.IsTrue(dialogs[0].States[1].DefaultTypes.Count == 3);
			Assert.IsTrue(dialogs[0].States[2].DefaultTypes.Count == 4);
			Assert.IsTrue(dialogs[6].Attributes.Count == 5);
#endif
		}

		[TestMethod]
		public void DefaultTypesStringTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual("a", defaults["&s0"]);
			Assert.AreEqual("b", defaults["s1"]);
			Assert.AreEqual("c", defaults["s2"]);
			Assert.AreEqual("d", defaults["s3"]);
		}

		[TestMethod]
		public void DefaultTypesBoolTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual(true, defaults["b1"]);
			Assert.AreEqual(false, defaults["b2"]);
			Assert.AreEqual(true, defaults["b3"]);
		}

		[TestMethod]
		public void DefaultTypesIntTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual(0, defaults["i1"]);
			Assert.AreEqual(1, defaults["i2"]);
			Assert.AreEqual(2, defaults["i3"]);
		}

		[TestMethod]
		public void DefaultTypesShortTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual((short)3, defaults["sh1"]);
			Assert.AreEqual((short)4, defaults["sh2"]);
			Assert.AreEqual((short)5, defaults["sh3"]);
		}

		[TestMethod]
		public void DefaultTypesLongTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual((long)6, defaults["l1"]);
			Assert.AreEqual((long)7, defaults["l2"]);
			Assert.AreEqual((long)8, defaults["l3"]);
		}

		[TestMethod]
		public void DefaultTypesFloatTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual((float)9, defaults["f1"]);
			Assert.AreEqual((float)10, defaults["f2"]);
			Assert.AreEqual((float)11, defaults["f3"]);
		}

		[TestMethod]
		public void DefaultTypesDoubleTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual((double)12, defaults["d1"]);
			Assert.AreEqual((double)13, defaults["d2"]);
			Assert.AreEqual((double)14, defaults["d3"]);
		}

		[TestMethod]
		public void DefaultTypesDecimalTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual((decimal)15, defaults["de1"]);
			Assert.AreEqual((decimal)16, defaults["de2"]);
			Assert.AreEqual((decimal)17, defaults["de3"]);
		}

		[TestMethod]
		public void DefaultTypesDateTimeTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual(new DateTime(1990, 3, 1, 12, 35, 47), defaults["dt1"]);
			Assert.AreEqual(new DateTime(1991, 4, 2, 13, 36, 48), defaults["dt2"]);
			Assert.AreEqual(new DateTime(1992, 5, 3, 14, 37, 49), defaults["dt3"]);
		}

		[TestMethod]
		public void DefaultTypesTimeSpanTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual("d", defaults["t1"]);
		}

		[TestMethod]
		public void DefaultTypesByteTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual((byte)18, defaults["by1"]);
			Assert.AreEqual((byte)19, defaults["by2"]);
			Assert.AreEqual((byte)20, defaults["by3"]);
		}

		[TestMethod]
		public void DefaultTypesCharTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual('e', defaults["ch1"]);
			Assert.AreEqual('f', defaults["ch2"]);
			Assert.AreEqual('g', defaults["ch3"]);
		}

		[TestMethod]
		public void DefaultTypesGuidTest()
		{
			StateInfoCollection<object> defaults = StateInfoConfig.Dialogs[1].States[1].Defaults;
			Assert.AreEqual("h", defaults["g1"]);
		}

#if NET40Plus
		[TestMethod]
		public void StateInfoConfigTest()
		{
			var dialogs = (StateInfoCollection<Dialog>)ConfigurationManager.GetSection("Navigation/StateInfo");
			foreach(Dialog dialogA in dialogs)
			{
				var dialogB = StateInfoConfig.Dialogs[dialogA.Index];
				Assert.AreEqual(dialogA.Key, dialogB.Key);
				Assert.AreEqual(dialogA.Initial.Key, dialogB.Initial.Key);
				Assert.AreEqual(dialogA.Path, dialogB.Path);
				Assert.AreEqual(dialogA.Title, dialogB.Title);
				foreach (string attributeKey in dialogA.Attributes.Keys)
				{
					if (attributeKey != "key" && attributeKey != "title" && attributeKey != "resourceKey" && attributeKey != "resourceType" && attributeKey != "initial")
						Assert.AreEqual(dialogA.Attributes[attributeKey], dialogB.Attributes[attributeKey]);
				}
				foreach (State stateA in dialogA.States)
				{
					var stateB = StateInfoConfig.Dialogs[dialogA.Index].States[stateA.Index];
					Assert.AreEqual(stateA.Key, stateB.Key);
					Assert.AreEqual(stateA.Route, stateB.Route);
					Assert.AreEqual(stateA.Page, stateB.Page);
					Assert.AreEqual(stateA.Title, stateB.Title);
					Assert.AreEqual(stateA.Theme, stateB.Theme);
					Assert.AreEqual(stateA.MobilePage, stateB.MobilePage);
					Assert.AreEqual(stateA.MobileRoute, stateB.MobileRoute);
					Assert.AreEqual(stateA.MobileTheme, stateB.MobileTheme);
					Assert.AreEqual(stateA.TrackCrumbTrail, stateB.TrackCrumbTrail);
					Assert.AreEqual(stateA.CheckPhysicalUrlAccess, stateB.CheckPhysicalUrlAccess);
					foreach (string defaultKey in stateA.Defaults.Keys)
						Assert.AreEqual(stateA.Defaults[defaultKey], stateB.Defaults[defaultKey]);
					foreach (string defaultTypeKey in stateA.DefaultTypes.Keys)
						Assert.AreEqual(stateA.DefaultTypes[defaultTypeKey], stateB.DefaultTypes[defaultTypeKey]);
					for (int i = 0; i < stateA.Masters.Count; i++)
						Assert.AreEqual(stateA.Masters[i], stateB.Masters[i]);
					for (int i = 0; i < stateA.MobileMasters.Count; i++)
						Assert.AreEqual(stateA.MobileMasters[i], stateB.MobileMasters[i]);
					foreach (string attributeKey in stateA.Attributes.Keys)
					{
						if (attributeKey != "key" && attributeKey != "title" && attributeKey != "resourceKey" && attributeKey != "resourceType"
							 && attributeKey != "defaults" && attributeKey != "defaultTypes" && attributeKey != "derived" && attributeKey != "trackCrumbTrail")
							Assert.AreEqual(stateA.Attributes[attributeKey], stateB.Attributes[attributeKey]);
					}
					foreach (Transition transitionA in stateA.Transitions)
					{
						var transitionB = StateInfoConfig.Dialogs[dialogA.Index].States[stateA.Index].Transitions[transitionA.Index];
						Assert.AreEqual(transitionA.Key, transitionB.Key);
						Assert.AreEqual(transitionA.To.Key, transitionB.To.Key);
					}
				}
			}
		}
#endif

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidTransitionToTest()
		{
			ConfigurationManager.GetSection("Navigation/InvalidTransitionTo");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidInitialTest()
		{
			ConfigurationManager.GetSection("Navigation/InvalidInitial");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void DuplicateDialogTest()
		{
			ConfigurationManager.GetSection("Navigation/DuplicateDialog");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void DuplicateStateTest()
		{
			ConfigurationManager.GetSection("Navigation/DuplicateState");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void DuplicateTransitionTest()
		{
			ConfigurationManager.GetSection("Navigation/DuplicateTransition");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void MissingDialogKeyTest()
		{
			ConfigurationManager.GetSection("Navigation/MissingDialogKey");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void EmptyDialogKeyTest()
		{
			ConfigurationManager.GetSection("Navigation/EmptyDialogKey");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void MissingDialogInitialTest()
		{
			ConfigurationManager.GetSection("Navigation/MissingDialogInitial");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void EmptyDialogInitialTest()
		{
			ConfigurationManager.GetSection("Navigation/EmptyDialogInitial");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void MissingStateKeyTest()
		{
			ConfigurationManager.GetSection("Navigation/MissingStateKey");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void EmptyStateKeyTest()
		{
			ConfigurationManager.GetSection("Navigation/EmptyStateKey");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void MissingTransitionKeyTest()
		{
			ConfigurationManager.GetSection("Navigation/MissingTransitionKey");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void EmptyTransitionKeyTest()
		{
			ConfigurationManager.GetSection("Navigation/EmptyTransitionKey");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void MissingTransitionToTest()
		{
			ConfigurationManager.GetSection("Navigation/MissingTransitionTo");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void EmptyTransitionToTest()
		{
			ConfigurationManager.GetSection("Navigation/EmptyTransitionTo");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidTrackCrumbTrailTest()
		{
			ConfigurationManager.GetSection("Navigation/InvalidTrackCrumbTrail");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void EmptyDefaultsTest()
		{
			ConfigurationManager.GetSection("Navigation/EmptyDefaults");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(FormatException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidSeparatorDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidSeparatorDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(FormatException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidEqualDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidEqualDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(FormatException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidKeyDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidKeyDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(FormatException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidTypeDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidTypeDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(FormatException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidCastGuidDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidCastGuidDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidCastException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidCastTimeSpanDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidCastTimeSpanDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidCastException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidFormatDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidFormatDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(FormatException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void OverflowDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/OverflowDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(OverflowException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void EmptyDefaultTypesTest()
		{
			ConfigurationManager.GetSection("Navigation/EmptyDefaultTypes");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidDefaultTypesTest()
		{
			ConfigurationManager.GetSection("Navigation/InvalidDefaultTypes");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidTypeDefaultTypesTest()
		{
			ConfigurationManager.GetSection("Navigation/InvalidTypeDefaultTypes");
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidGuidDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidGuidDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidCastException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void InvalidTimeSpanDefaultsTest()
		{
			try
			{
				ConfigurationManager.GetSection("Navigation/InvalidTimeSpanDefaults");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidCastException));
				throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ParseNavigationDataExpressionNullExpressionTest()
		{
			StateInfoConfig.ParseNavigationDataExpression(null, null, false);
		}

		[TestMethod]
		public void ParseNavigationDataExpressionNullStateTest()
		{
			NavigationData data = StateInfoConfig.ParseNavigationDataExpression("x=y", null, false);
			Assert.AreEqual("y", data["x"]);
		}

		[TestMethod]
		[ExpectedException(typeof(FormatException))]
		public void ParseNavigationDataExpressionEmptyExpressionTest()
		{
			NavigationData data = StateInfoConfig.ParseNavigationDataExpression("", null, false);
		}
	}
}
