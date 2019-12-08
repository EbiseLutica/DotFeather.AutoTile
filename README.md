# DotFeather.AutoTile

DotFeather.AutoTile は、ウディタ向けオートタイル画像をDotFeatherで使用できるプラグインです。

## 使い方

1 ソースファイルだけなので、 [AutoTile.cs](AutoTile.cs) をコピーし、プロジェクトに読み込んで、次のように初期化してください。

```cs
AutoTile tile = AutoTile.LoadFrom("./dirt.png", 1, new VectorInt(16, 16));
```

ウディタ向けに提供されるオートタイルをそのまま利用できます。

## ライセンス

MIT ライセンスです。ソースファイルに記載されている著作権表記を削除せずそのまま使用してください。