package com.tlp.tlplib.util;

@SuppressWarnings("unused")
public class StringUtil {
    public static String shiftCharValues(String s, int shiftBy) {
        char[] chars = s.toCharArray();
        char[] shifted = new char[chars.length];
        for (int idx = 0; idx < chars.length; idx++) {
            shifted[idx] = (char) (chars[idx] + shiftBy);
        }
        return new String(shifted);
    }
}
