﻿using Microsoft.VisualStudio.Modeling.Validation;
using Navigation.Designer.CustomCode.Validation;
using System.Collections.Generic;
using System.Linq;

namespace Navigation.Designer
{
	[ValidationState(ValidationState.Enabled)]
	public partial class NavigationDiagram
	{
		[ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
		private void Validate(ValidationContext context)
		{
			StateInfo stateInfo = new StateInfo();
			List<Dialog> dialogs = stateInfo.Convert(this);
			ValidateDialogKey(context, dialogs);
			ValidateStateKey(context, dialogs);
			ValidateTransitionKey(context, dialogs);
			ValidatePathAndRoute(context, dialogs);
		}

		private void ValidateDialogKey(ValidationContext context, List<Dialog> dialogs)
		{
			var q = from d in dialogs
					where !string.IsNullOrEmpty(d.Key)
					group d.Initial by d.Key into g
					where g.Count() > 1
					select g;
			foreach (var d in q)
			{
				context.LogError(string.Format(Messages.DuplicateDialogKey, d.Key), "DuplicateDialogKey", d.ToArray());
			}
		}

		private void ValidateStateKey(ValidationContext context, List<Dialog> dialogs)
		{
			foreach (Dialog dialog in dialogs)
			{
				var q = from s in dialog.States
						where !string.IsNullOrEmpty(s.Key)
						group s.State by s.Key into g
						where g.Count() > 1
						select g;
				foreach (var s in q)
				{
					context.LogError(string.Format(Messages.DuplicateStateKey, dialog.Key, s.Key), "DuplicateStateKey", s.ToArray());
				}
			}
		}

		private void ValidateTransitionKey(ValidationContext context, List<Dialog> dialogs)
		{
			var q = from d in dialogs
					from s in d.States
					from t in Transition.GetLinksToPredecessors(s.State)
					where d.Initial == s.State
					&& t.Key != d.Key
					&& !t.CanNavigateBack
					select new { t, d };
			foreach(var t in q)
			{
				context.LogWarning(string.Format(Messages.TransitionKeyRename, t.t.Key, t.d.Key), "TransitionKeyRename", t.t);

			}
		}

		private void ValidatePathAndRoute(ValidationContext context, List<Dialog> dialogs)
		{
			if (dialogs.Count > 0)
			{
				bool noPathOrRoute = dialogs.Where(d => !string.IsNullOrEmpty(d.Path + d.Initial.Route)).FirstOrDefault() == null;
				if (noPathOrRoute)
				{
					context.LogError(Messages.PathAndRouteEmpty, "PathAndRouteEmpty", (dialogs.Select(d => d.Initial).ToArray()));
				}
			}
		}
	}
}
