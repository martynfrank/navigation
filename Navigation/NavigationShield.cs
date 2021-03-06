using System.Collections.Specialized;
using System.Configuration;

namespace Navigation
{
	/// <summary>
	/// Provides the base functionality for Url protection mechanisms e.g. to prevent tampering or
	/// to obfuscate query string parameters. Regardless of the mechanism the state id parameter,
	/// <see cref="Navigation.NavigationSettings.StateIdKey"/>, is always present
	/// </summary>
	public abstract class NavigationShield : ConfigurationSection
	{
#if NET35Plus
		/// <summary>
		/// Overridden by derived classes to return a protected set of query string parameters
		/// </summary>
		/// <param name="data">An unprotected set of key/value pairs prior to the formation
		/// of the querty string</param>
		/// <param name="historyPoint">Identifies if the Url is for history navigation</param>
		/// <param name="state">The <see cref="Navigation.State"/> the Url will navigate to</param>
		/// <returns>Protected set of query string parameters</returns>
		public virtual NameValueCollection Encode(NameValueCollection data, bool historyPoint, State state)
		{
			return Encode(data, historyPoint);
		}

		/// <summary>
		/// Overridden by derived classes to return a protected set of query string parameters
		/// </summary>
		/// <param name="data">An unprotected set of key/value pairs prior to the formation
		/// of the querty string</param>
		/// <param name="historyPoint">Identifies if the Url is for history navigation</param>
		/// <returns>Protected set of query string parameters</returns>
		public abstract NameValueCollection Encode(NameValueCollection data, bool historyPoint);
#else
		/// <summary>
		/// Overridden by derived classes to return a protected set of query string parameters
		/// </summary>
		/// <param name="data">An unprotected set of key/value pairs prior to the formation
		/// of the querty string</param>
		/// <param name="state">The <see cref="Navigation.State"/> the Url will navigate to</param>
		/// <returns>Protected set of query string parameters</returns>
		public virtual NameValueCollection Encode(NameValueCollection data, State state)
		{
			return Encode(data);
		}

		/// <summary>
		/// Overridden by derived classes to return a protected set of query string parameters
		/// </summary>
		/// <param name="data">An unprotected set of key/value pairs prior to the formation
		/// of the querty string</param>
		/// <returns>Protected set of query string parameters</returns>
		public abstract NameValueCollection Encode(NameValueCollection data);
#endif

#if NET35Plus
		/// <summary>
		/// Overridden by derived classes to return an unprotected set of query string parameters
		/// </summary>
		/// <param name="data">A protected set of key/value pairs produced by the Encode method</param>
		/// <param name="historyPoint">Identifies if the Url is being decoded as a result of
		/// a call to <see cref="StateController.NavigateHistory"/></param>
		/// <param name="state">The <see cref="Navigation.State"/> the Url has navigated to</param>
		/// <returns>Unprotected set of query string parameters</returns>
		public virtual NameValueCollection Decode(NameValueCollection data, bool historyPoint, State state)
		{
			return Decode(data, historyPoint);
		}

		/// <summary>
		/// Overridden by derived classes to return an unprotected set of query string parameters
		/// </summary>
		/// <param name="data">A protected set of key/value pairs produced by the Encode method</param>
		/// <param name="historyPoint">Identifies if the Url is being decoded as a result of
		/// a call to <see cref="StateController.NavigateHistory"/></param>
		/// <returns>Unprotected set of query string parameters</returns>
		public abstract NameValueCollection Decode(NameValueCollection data, bool historyPoint);
#else
		/// <summary>
		/// Overridden by derived classes to return an unprotected set of query string parameters
		/// </summary>
		/// <param name="data">A protected set of key/value pairs produced by the Encode method</param>
		/// <param name="state">The <see cref="Navigation.State"/> the Url has navigated to</param>
		/// <returns>Unprotected set of query string parameters</returns>
		public virtual NameValueCollection Decode(NameValueCollection data, State state)
		{
			return Decode(data);
		}

		/// <summary>
		/// Overridden by derived classes to return an unprotected set of query string parameters
		/// </summary>
		/// <param name="data">A protected set of key/value pairs produced by the Encode method</param>
		/// <returns>Unprotected set of query string parameters</returns>
		public abstract NameValueCollection Decode(NameValueCollection data);
#endif
	}
}
