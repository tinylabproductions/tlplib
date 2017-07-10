package com.tinylabproductions.tlplib.crash_reporting;

import java.util.List;

@SuppressWarnings({"unused", "WeakerAccess"})
public abstract class UnityMessageBase extends Throwable {
    protected UnityMessageBase(String message) { super(message); }

    // Can't call setStackTrace directly, because
    // http://forum.unity3d.com/threads/passing-arrays-through-the-jni.91757/#post-1899528
    public void setStackTraceElems(List<StackTraceElement> elems) {
        StackTraceElement[] arr = new StackTraceElement[elems.size()];
        setStackTrace(elems.toArray(arr));
    }
}
