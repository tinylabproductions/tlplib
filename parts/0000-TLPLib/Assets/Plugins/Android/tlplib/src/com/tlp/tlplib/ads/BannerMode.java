package com.tlp.tlplib.ads;

public class BannerMode {
    public interface Mode {}

    public static class FixedSize implements Mode {
        public final int width, height;

        public FixedSize(int width, int height) {
            this.width = width;
            this.height = height;
        }
    }

    public static class PercentileSize implements Mode {
        public final float width, height;

        public PercentileSize(float width, float height) {
            this.width = width;
            this.height = height;
        }
    }

    public static class WrapContent implements Mode {
        public final static WrapContent instance = new WrapContent();
        private WrapContent() {}
    }
}
