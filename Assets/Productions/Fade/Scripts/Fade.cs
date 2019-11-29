/*
 The MIT License (MIT)

Copyright (c) 2013 yamamura tatsuhiko

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class Fade : MonoBehaviour
{
	IFade fade;

	void Start ()
	{
		Init ();
        // ここに1.0fを代入したら初期に黒画面から始まる
		fade.Range = cutout_range = 1.0f;
	}

	float cutout_range;

	void Init ()
	{
		fade = GetComponent<IFade> ();
	}

	void OnValidate ()
	{
		Init ();
		fade.Range = cutout_range;
	}

	IEnumerator FadeoutCoroutine (float time, System.Action action)
	{
		float endTime = Time.timeSinceLevelLoad + time * (cutout_range);

		var endFrame = new WaitForEndOfFrame ();

		while (Time.timeSinceLevelLoad <= endTime)
        {
			cutout_range = (endTime - Time.timeSinceLevelLoad) / time;
			fade.Range = cutout_range;
			yield return endFrame;
		}
		cutout_range = 0;
		fade.Range = cutout_range;

		if (action != null)
        {
			action ();
		}
	}

	IEnumerator FadeinCoroutine (float time, System.Action action)
	{
		float endTime = Time.timeSinceLevelLoad + time * (1 - cutout_range);
		
		var endFrame = new WaitForEndOfFrame ();

		while (Time.timeSinceLevelLoad <= endTime)
        {
			cutout_range = 1 - ((endTime - Time.timeSinceLevelLoad) / time);
			fade.Range = cutout_range;
			yield return endFrame;
		}
		cutout_range = 1;
		fade.Range = cutout_range;

		if (action != null)
        {
			action ();
		}
	}

	public Coroutine FadeOut (float time, System.Action action)
	{
		StopAllCoroutines ();
		return StartCoroutine (FadeoutCoroutine (time, action));
	}

	public Coroutine FadeOut (float time)
	{
		return FadeOut (time, null);
	}

	public Coroutine FadeIn (float time, System.Action action)
	{
		StopAllCoroutines ();
		return StartCoroutine (FadeinCoroutine (time, action));
	}

	public Coroutine FadeIn (float time)
	{
		return FadeIn (time, null);
	}
}