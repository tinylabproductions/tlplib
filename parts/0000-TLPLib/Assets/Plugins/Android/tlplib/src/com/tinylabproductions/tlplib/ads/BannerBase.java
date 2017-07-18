package com.tinylabproductions.tlplib.ads;

import android.app.Activity;
import android.util.Log;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import com.tinylabproductions.tlplib.fns.Fn1;

@SuppressWarnings("unused")
public abstract class BannerBase<Banner extends View> implements IStandardBanner {
    protected abstract String TAG();

    protected final Activity activity;

    protected Banner banner;

    protected BannerBase(
            Activity activity,
            final boolean isTopBanner, final BannerMode.Mode mode,
            final Fn1<Banner> createBanner
    ) {
        this(activity, isTopBanner, mode, createBanner, true);
    }

    protected BannerBase(
            Activity activity, final boolean isTopBanner, final BannerMode.Mode mode,
            final Fn1<Banner> createBanner, final boolean preloadBanner
    ) {
        this.activity = activity;

        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                banner = createBanner.run();
                addToUI(mode, isTopBanner, preloadBanner);
            }
        });
    }

    protected void addToUI(BannerMode.Mode mode, boolean isTopBanner, boolean preloadBanner) {
        int finalWidth, finalHeight;

        if (mode instanceof BannerMode.WrapContent) {
            finalWidth = FrameLayout.LayoutParams.MATCH_PARENT;
            finalHeight = FrameLayout.LayoutParams.WRAP_CONTENT;
        }
        else if (mode instanceof BannerMode.FixedSize) {
            BannerMode.FixedSize _mode = (BannerMode.FixedSize) mode;
            float density = activity.getResources().getDisplayMetrics().density;
            finalWidth = (int)(_mode.width * density);
            finalHeight = (int)(_mode.height * density);
        }
        else {
            throw new RuntimeException("Unknown banner mode: " + mode);
        }

        int gravity = (isTopBanner ? Gravity.TOP : Gravity.BOTTOM) | Gravity.CENTER_HORIZONTAL;
        final FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(
                finalWidth, finalHeight, gravity
        );
        Log.d(
                TAG(),
                "Adding banner to frame [width:"+finalWidth+" height:"+finalHeight+" gravity:"+
                        gravity+"]"
        );

        activity.addContentView(banner, params);

        Log.d(TAG(), "Banner added to UI.");
        setVisibilityRunsOnUiThread(!preloadBanner);
    }

    @Override
    public final void load() {
        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                loadRunsOnUiThread();
            }
        });
    }

    protected abstract void loadRunsOnUiThread();

    protected void setVisibilityRunsOnUiThread(boolean visible) {
        if (banner != null) {
            banner.setVisibility(visible ? View.VISIBLE : View.GONE);
            Log.d(TAG(), "Banner visible=" + visible);
        }
        else Log.d(TAG(), "Banner frame is null, can't set visibility");
    }

    @SuppressWarnings("unused")
    public void setVisibility(final boolean visible) {
        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                setVisibilityRunsOnUiThread(visible);
            }
        });
    }

    @SuppressWarnings("unused")
    public final void destroy() {
        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                destroyRunsOnUiThread();
            }
        });
    }

    protected void destroyRunsOnUiThread() {
        if (banner == null) return;
        ViewGroup parent = (ViewGroup) banner.getParent();
        if (parent != null) {
            beforeDestroyRunsOnUiThread();
            parent.removeView(banner);
            afterDestroyRunsOnUiThread();
        }
        banner = null;
    }

    protected void beforeDestroyRunsOnUiThread() {}
    protected void afterDestroyRunsOnUiThread() {}
}
