# Shipwreck.HlsDownloader

WebView based M3U8 (HTTP Live Streaming) downloader.

## Usage

1. Build the solution. (TODO: easier deployment)
2. Open a URL of a HTML that provides HLS.
3. Play HLS in the Web View. (The Web View can't play the media actually, but attempts to download first TS segment.)
4. If the page requests remote M3U8 files, these will be shown in a grid below. Single HLS media normally contains 2 `.m3u8`s.
5. Select the latter `.m3u8` and click the `Select .m3u8` button.
6. Enter a local path to save `.m3u8` and click the `Save` button. The `.m3u8` and `.ts` files will be downloaded in the specified directory.
7. Or you can use [FFmpeg](https://www.ffmpeg.org/) to concatenate the `.ts` directly.

## License

MIT