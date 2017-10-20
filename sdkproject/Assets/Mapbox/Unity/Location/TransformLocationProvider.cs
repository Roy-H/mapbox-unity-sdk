namespace Mapbox.Unity.Location
{
	using System;
	using Mapbox.Unity.Map;
	using Mapbox.Unity.Utilities;
	using UnityEngine;

	/// <summary>
	/// The TransformLocationProvider is responsible for providing mock location and heading data
	/// for testing purposes in the Unity editor.
	/// This is achieved by querying a Unity <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> every frame.
	/// You might use this to to update location based on a touched position, for example.
	/// </summary>
	public class TransformLocationProvider : AbstractLocationProvider
	{
		[SerializeField]
		private AbstractMap _map;

		/// <summary>
		/// The transform that will be queried for location and heading data.
		/// </summary>
		[SerializeField]
		Transform _targetTransform;

		[SerializeField]
		int _accuracy;

		Location _currentLocation;

		/// <summary>
		/// Sets the target transform.
		/// Use this if you want to switch the transform at runtime.
		/// </summary>
		public Transform TargetTransform
		{
			set
			{
				_targetTransform = value;
			}
		}

#if UNITY_EDITOR
		void Update()
        {
			_currentLocation.Heading = _targetTransform.eulerAngles.y;
			_currentLocation.LatitudeLongitude = _targetTransform.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
			_currentLocation.Accuracy = _accuracy;
			_currentLocation.Timestamp = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			SendLocation(_currentLocation);
        }
#endif
    }
}
