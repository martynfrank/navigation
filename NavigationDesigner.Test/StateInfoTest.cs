﻿using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Navigation.Designer;
using System.Collections.Generic;
using System.Linq;

namespace Navigation.Designer.Test
{
	[TestClass]
	public class StateInfoTest
	{
		private static NavigationDiagram LoadModel(string navigationModel)
		{
			Store store = new Store(typeof(NavigationLanguageDomainModel));
			NavigationDiagram navigationConfiguration;
			using (Transaction t = store.TransactionManager.BeginTransaction("test", true))
			{
				navigationConfiguration = NavigationLanguageSerializationHelper.Instance.LoadModel(store, navigationModel, null, null, null);
				t.Commit();
			}
			return navigationConfiguration;
		}

		private static bool ValidateNavigation(NavigationDiagram navigationConfiguration, List<Dialog> dialogs)
		{
			var unconvertedNavigations =
				from s in navigationConfiguration.States
				from su in s.Successors
				let c = GetStateWrappersCanNavigateTo(dialogs, s, su).Count()
				where (c > 0 && c < GetStateWrappers(dialogs, s).Count())
				|| (c == 0 && dialogs.Where(d => d.Initial == su).FirstOrDefault() == null)
				|| (c == 0 && Transition.GetLink(s, su).CanNavigateBack)
				select s;
			var unconvertedInitialStates =
				from s in navigationConfiguration.States
				where s.Initial
				&& dialogs.Where(d => d.Initial == s).FirstOrDefault() == null
				select s;
			return unconvertedNavigations.FirstOrDefault() == null && unconvertedInitialStates.FirstOrDefault() == null;
		}

		private static bool ValidateTransition(NavigationDiagram navigationConfiguration, List<Dialog> dialogs)
		{
			var invalidTransitions =
				from d in dialogs
				from s in d.States
				from t in s.Transitions
				let n = Transition.GetLink(s.State, t.To.State)
				where n == null || n.Key != t.Key
				select t;
			return invalidTransitions.FirstOrDefault() == null;
		}

		private static IEnumerable<StateWrapper> GetStateWrappersCanNavigateTo(List<Dialog> dialogs, State state, State successor)
		{
			return from sw in GetStateWrappers(dialogs, state)
				   where sw.CanNavigateTo(successor, Transition.GetLink(state, successor).Key)
				   select sw;
		}

		private static IEnumerable<StateWrapper> GetStateWrappers(List<Dialog> dialogs, State state)
		{
			return from d in dialogs
				   from s in d.States
				   where s.State == state
				   select s;
		}

		[TestMethod]
		public void AnBnCMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnC.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void A_B_CMakes3DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/A_B_C.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(3, dialogs.Count());
		}

		[TestMethod]
		public void AnB_CnDMakes2DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnB_CnD.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(2, dialogs.Count());
		}

		[TestMethod]
		public void AnB_An1CMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnB_An1C.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnB_CnBMakes3DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnB_CnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(3, dialogs.Count());
		}

		[TestMethod]
		public void AnbB_CnBMakes2DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnbB_CnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(2, dialogs.Count());
		}

		[TestMethod]
		public void AnbB_CnbBMakes2DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnbB_CnbB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(2, dialogs.Count());
		}

		[TestMethod]
		public void AnbB_CnbB_BiMakes3DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnbB_CnbB_Bi.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(3, dialogs.Count());
		}

		[TestMethod]
		public void AnBnCnAMakes1DialogATest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnA.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
			Assert.IsNotNull(dialogs.Where(d => d.Initial.Key == "A").FirstOrDefault());
		}

		[TestMethod]
		public void AnBnCnA_BiMakes1DialogBTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnA_Bi.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
			Assert.IsNotNull(dialogs.Where(d => d.Initial.Key == "B").FirstOrDefault());
		}

		[TestMethod]
		public void AnBnCnDnBMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnDnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnBnCnA_DnBMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnA_DnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnBnbCnA_DnbBMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnbCnA_DnbB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnB_CnB_BnD_EnDnFMakes5DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnB_CnB_BnD_EnDnF.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(5, dialogs.Count());
		}

		[TestMethod]
		public void AnB_CnBnDnBMakes3DialogsNotDTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnB_CnBnDnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(3, dialogs.Count());
			Assert.IsNull(dialogs.Where(d => d.Initial.Key == "D").FirstOrDefault());
		}

		[TestMethod]
		public void AnbB_CnBnDnBMakes4DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnbB_CnBnDnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(4, dialogs.Count());
		}

		[TestMethod]
		public void AnbB_CnBnDnBMakesACBDDialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnbB_CnBnDnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual("A", dialogs[0].Key);
			Assert.AreEqual("C", dialogs[1].Key);
			Assert.AreEqual("B", dialogs[2].Key);
			Assert.AreEqual("D", dialogs[3].Key);
		}

		[TestMethod]
		public void AnbB_CnBnbDnBMakes2DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnbB_CnBnbDnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(2, dialogs.Count());
		}

		[TestMethod]
		public void AnBnCnDnB_Cn1EnFnGnHnFMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnDnB_Cn1EnFnGnHnF.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnBnCnAn1Cn1Bn1AMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnAn1Cn1Bn1A.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnBnC_AiBiMakes2DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnC_AiBi.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(2, dialogs.Count());
		}

		[TestMethod]
		public void AnBnC_AiBiCiMakes3DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnC_AiBiCi.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(3, dialogs.Count());
		}

		[TestMethod]
		public void AnBnDnB_CnbD_DiMakes4DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnDnB_CnbD_Di.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(4, dialogs.Count());
		}

		[TestMethod]
		public void AnbBnbCnbDnbC_BiMakes2DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnbBnbCnbDnbC_Bi.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(2, dialogs.Count());
		}

		[TestMethod]
		public void AnBnCnAn1Cn1Bn1An1CMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnAn1Cn1Bn1An1C.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnBnCnAn1Cn1B_CiMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnAn1Cn1B_Ci.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void BnCnBn1AMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/BnCnBn1A.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void BnCnBn1bA_DnA_EnAMakes4DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/BnCnBn1bA_DnA_EnA.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(4, dialogs.Count());
		}

		[TestMethod]
		public void AnBnCnCMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnC.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnBnCnCn1AMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnCn1A.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnbBnB_CnBMakes2DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnbBnB_CnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(2, dialogs.Count());
		}

		[TestMethod]
		public void AnBnB_CnBMakes3DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnB_CnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(3, dialogs.Count());
		}

		[TestMethod]
		public void AnBnbB_CnBMakes3DialogsTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnbB_CnB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(3, dialogs.Count());
		}

		[TestMethod]
		public void AnBnCnDnBn1B_Cn1CMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnBnCnDnBn1B_Cn1C.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnAMakes1DialogTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnA.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.IsTrue(ValidateNavigation(navigationConfiguration, dialogs));
			Assert.IsTrue(ValidateTransition(navigationConfiguration, dialogs));
			Assert.AreEqual(1, dialogs.Count());
		}

		[TestMethod]
		public void AnbBirKeepsOnlyInitialRouteTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/AnbBir.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.AreEqual(string.Empty, dialogs.First(d => d.Initial.Key == "A").States.First(s => s.Key == "B").Route);
			Assert.AreEqual("B", dialogs.First(d => d.Initial.Key == "B").States.First(s => s.Key == "B").Route);
		}

		[TestMethod]
		public void A1nbB_CnbBKeepsOnlyCRouteTest()
		{
			NavigationDiagram navigationConfiguration = LoadModel("Diagram/A1nbB_CnbB.nav");
			List<Dialog> dialogs = new StateInfo().Convert(navigationConfiguration);
			Assert.AreEqual(string.Empty, dialogs.First(d => d.Initial.Key == "A").States.First(s => s.Key == "B").Route);
			Assert.AreEqual("B", dialogs.First(d => d.Initial.Key == "C").States.First(s => s.Key == "B").Route);
		}
	}
}
