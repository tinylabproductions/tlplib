package com.tinylabproductions.tlplib.util;

import java.util.HashMap;

// HashMap that can be efficiently passed from C# side without allocating a bunch of tiny objects.
@SuppressWarnings("unused")
public class GCFreeHashMap<K, V> {
    @SuppressWarnings("WeakerAccess")
    public final HashMap<K, V> map;

    public GCFreeHashMap(K[] keys, V[] values) {
        if (keys.length != values.length) throw new IllegalArgumentException(
          "keys size (" + keys.length + ") != values size (" + values.length + ")"
        );

        map = new HashMap<>(keys.length);
        for (int idx = 0; idx < keys.length; idx++) {
            map.put(keys[idx], values[idx]);
        }
    }
}
