using UnityEngine;
using System;
using System.Collections;


public class FloatCallbackTweenProperty : AbstractTweenProperty, IGenericProperty
{
	public string propertyName { get; private set; }
	private Action<float> _setter;
	
	protected float _startValue;
	protected float _endValue;
	protected float _diffValue;


  public FloatCallbackTweenProperty(Action<float> setter, float startValue, float endValue, bool isRelative = false)
    : base(isRelative) {
    _setter = setter;
    _startValue = startValue;
    _endValue = endValue;
	}

	
	public override void prepareForUse()
	{
		
		// if this is a from tween we need to swap the start and end values
		if( _ownerTween.isFrom ) {
		  var t = _endValue;
		  _endValue = _startValue;
		  _startValue = t;
		}
		
		// setup the diff value
		if( _isRelative && !_ownerTween.isFrom )
			_diffValue = _endValue;
		else
			_diffValue = _endValue - _startValue;
	}
	

	public override void tick( float totalElapsedTime )
	{
		var easedValue = _easeFunction( totalElapsedTime, _startValue, _diffValue, _ownerTween.duration );
		_setter( easedValue );
	}
}
