﻿namespace Mapbox.Examples
{
    using Mapbox.Unity.Map;
    using Mapbox.Unity.Utilities;
    using Mapbox.Utils;
    using UnityEngine;
    using System;

    public class QuadTreeCameraMovement : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        public float _zoomSpeed = 50f;

        [SerializeField]
        //[HideInInspector]
        public Camera _referenceCamera;

        [SerializeField]
        QuadTreeTileProvider _quadTreeTileProvider;

        [SerializeField]
        AbstractMap _dynamicZoomMap;

        private Vector3 _origin;
		Vector3 _mousePosition;
		Vector3 _mousePositionPrevious;
        bool _shouldDrag;
        private Transform _originalCameraPosition;

        /// <summary>min of y range camera is allowed to move in</summary>
        private int _cameraZoomingRangeMinY;
        public int CameraZoomingRangeMinY { get { return _cameraZoomingRangeMinY; } }
        /// <summary>max of y range camera is allowed to move in</summary>
        private int _cameraZoomingRangeMaxY;

        public int CameraZoomingRangeMaxY { get { return _cameraZoomingRangeMaxY; } }

        void Start()
        {
            if (null == _referenceCamera)
            {
                _referenceCamera = GetComponent<Camera>();
                if (null == _referenceCamera) { Debug.LogErrorFormat("{0}: reference camera not set", this.GetType().Name); }
            }

            //put camera facing down. 
            //_referenceCamera.transform.rotation = new Quaternion(0.7f, 0, 0, 0.7f);

            //link zoomspeed to tilesize
            _zoomSpeed = _dynamicZoomMap.UnityTileSize / 2f;

            _cameraZoomingRangeMaxY = (int)(_dynamicZoomMap.UnityTileSize * 2.5f);
            _cameraZoomingRangeMinY = (int)(_dynamicZoomMap.UnityTileSize * 1.25f);
        }


        private void LateUpdate()
        {
            if (null == _dynamicZoomMap) { return; }


            //development short cut: reset center to 0/0 via right click
            //if (Input.GetMouseButton(1))
            //{
            //	_dynamicZoomMap.SetCenterMercator(Vector2d.zero);
            //	return;
            //}


            // zoom
            var scrollDelta = Input.GetAxis("Mouse ScrollWheel");

            if (scrollDelta > 0f)
            {
                _dynamicZoomMap.SetZoomRange(Mathf.Min(_dynamicZoomMap.ZoomRange + 0.25f, 21.0f));
            }
            else if (scrollDelta < 0f)
            {
                _dynamicZoomMap.SetZoomRange(Mathf.Max(_dynamicZoomMap.ZoomRange - 0.25f, 0.0f));
            }


            //pan keyboard
            float xMove = Input.GetAxis("Horizontal");
            float zMove = Input.GetAxis("Vertical");
            if (Math.Abs(xMove) > 0.0f || Math.Abs(zMove) > 0.0f)
            {
                float factor = Conversions.GetTileScaleInMeters((float)_dynamicZoomMap.CenterLatitudeLongitude.x, _dynamicZoomMap.Zoom) / (2.0f * _dynamicZoomMap.Zoom * _dynamicZoomMap.UnityTileSize);
                Debug.Log("Keyboard panning" + xMove + " , " + zMove + " Factor : " + factor);

                //TODO : Compare performance of panning in LateUpdate vs Update of TileProvider!
                //double xDelta = _dynamicZoomMap.CenterLatitudeLongitude.x + zMove * factor;
                //double zDelta = _dynamicZoomMap.CenterLatitudeLongitude.y + xMove * factor;

                //xDelta = xDelta > 0 ? Mathd.Min(xDelta, Mapbox.Utils.Constants.WebMercMax) : Mathd.Max(xDelta, -Mapbox.Utils.Constants.WebMercMax);
                //zDelta = zDelta > 0 ? Mathd.Min(zDelta, Mapbox.Utils.Constants.WebMercMax) : Mathd.Max(zDelta, -Mapbox.Utils.Constants.WebMercMax);


                ////_dynamicZoomMap.SetCenterMercator(_dynamicZoomMap.CenterMercator - new Vector2d(xMove, zMove));
                //_dynamicZoomMap.SetCenterLatitudeLongitude(new Vector2d(xDelta, zDelta));
                //_quadTreeTileProvider.UpdateMapProperties(0);



                _quadTreeTileProvider.SetPanRange(new Vector2d(xMove * factor, zMove * factor));
            }

            //pan mouse
            if (Input.GetMouseButton(0))
            {
                var mousePosScreen = Input.mousePosition;
                //assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
                //http://answers.unity3d.com/answers/599100/view.html
                mousePosScreen.z = _referenceCamera.transform.localPosition.y;
                _mousePosition = _referenceCamera.ScreenToWorldPoint(mousePosScreen);
                _mousePosition.y = 0f;
                if (_shouldDrag == false)
                {
                    _shouldDrag = true;
                    _origin = _referenceCamera.ScreenToWorldPoint(mousePosScreen);
                }
            }
            else
            {
                _shouldDrag = false;
            }

            if (_shouldDrag == true)
            {
				var changeFromPreviousPosition = _mousePositionPrevious - _mousePosition;
				if (Mathf.Abs(changeFromPreviousPosition.x) > 0.0f || Mathf.Abs(changeFromPreviousPosition.z) > 0.0f)
				{
					_mousePositionPrevious = _mousePosition;
					var offset = _origin - _mousePosition;
					if (Mathf.Abs(offset.x) > 0.0f || Mathf.Abs(offset.z) > 0.0f)
					{
						if (null != _dynamicZoomMap)
						{
							float factor = Conversions.GetTileScaleInMeters((float)_dynamicZoomMap.CenterLatitudeLongitude.x, _dynamicZoomMap.Zoom) / (256.0f * _dynamicZoomMap.Zoom * _dynamicZoomMap.UnityTileSize);
							_quadTreeTileProvider.SetPanRange(new Vector2d(offset.x * factor, offset.z * factor));

							UnityEngine.Debug.Log("Dragging : " + factor);
						}
					}
				}
            }
        }
    }
}