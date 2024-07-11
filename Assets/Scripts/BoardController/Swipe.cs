using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Swipe : MonoBehaviour
{
    private bool _tap, _swipeLeft, _swipeRight, _swipeUp, _swipeDown;
    private bool _isDragging;
    private Vector2 _startTouch, _swipeDelta;

    public bool Tap { get { return _tap; } }
    public bool SwipeLeft { get { return _swipeLeft; } }
    public bool SwipeRight { get { return _swipeRight; } }
    public bool SwipeUp { get { return _swipeUp; } }
    public bool SwipeDown { get { return _swipeDown; } }
    public Vector2 SwipeDelta { get { return _swipeDelta; } }

    private void Reset()
    {
        _isDragging = false;
        _startTouch = _swipeDelta = Vector2.zero;
    }

    private void Update()
    {
        _tap = _swipeLeft = _swipeRight = _swipeUp = _swipeDown = false;

        #region Standalone Inputs
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;

            _tap = true;
            _startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0) || _swipeDelta.magnitude > 50)
        {
            _isDragging = false;

            Reset();
        }
        #endregion

        #region Mobile Inputs
        if (Input.touches.Length > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                _isDragging = true;

                _tap = true;
                _startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled
            || _swipeDelta.magnitude > 150)
            {
                _isDragging = false;

                Reset();
            }
        }
        #endregion

        _swipeDelta = Vector2.zero;

        if (_isDragging)
        {
            if (Input.touches.Length > 0)
            {
                _swipeDelta = Input.touches[0].position - _startTouch;
            }
            else if (Input.GetMouseButton(0))
            {
                _swipeDelta = (Vector2)Input.mousePosition - _startTouch;
            }
        }

        if (_swipeDelta.magnitude > 50)
        {
            float x = _swipeDelta.x;
            float y = _swipeDelta.y;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                if (x < 0)
                {
                    _swipeLeft = true;
                }
                else
                {
                    _swipeRight = true;
                }
            }
            else
            {
                if (y < 0)
                {
                    _swipeDown = true;
                }
                else
                {
                    _swipeUp = true;
                }
            }
        }
    }
}
