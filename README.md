# UserMovingFloor

VRChat用乗り物床面追従システム

鉄道やバスなど乗客が移動出来る床面が存在し緩やかに発停車する乗り物に適しています。

## インストール

**以下の前提リソースを事前にインポートして下さい**
- [VRCSDK3-World](https://vrchat.com/home/download)
- [UdonSharp](https://github.com/MerlinVR/UdonSharp/releases)
- [CyanPlayerObjectPool](https://github.com/CyanLaser/CyanPlayerObjectPool/releases/tag/v0.0.5)

**[UserMovingFloor ダウンロード](https://github.com/Narazaka/VRChat_UserMovingFloor/releases)**

## 使い方

0. あらかじめ [CyanPlayerObjectPool](https://github.com/CyanLaser/CyanPlayerObjectPool/releases/tag/v0.0.5) をインポートしておいて下さい。
1. 乗り物のオブジェクトにUser Moving Floor Targetを付けます。
   - Ride Colliders:（任意）乗り物が停止しているときに有効な床面などのコライダー。乗車中に干渉しないようOFFになります。
   - Inside Collider: （必須）乗り物の内部に居る判定用コライダー。IsTriggerを有効にしてください。
   - Can Get Off While Moving: （任意）乗り物が走行中にInside Collider範囲外に出たときに下車扱いにするならtrue。
   - Moving: （必須）乗り物が走行中ならばtrue。スクリプト等で動的に動かす際にここを制御して下さい。
   - User Moving Floor Targetと同じInsideCollider, CanGetOffWhileMoving, Moving, _OnGetOn, _OnGetOffのインターフェースを持った任意のUdonBehaviorを代わりに設定しても良いです。
1. UserMovingFloor.prefabをシーンに置きます。
2. PlayerObjectPoolオブジェクトが作成されるので、Pool Sizeを(ワールドの人数上限*2+2)にしてください。
3. UserMovingFloorのTargetsにUser Moving Floor Targetが付いた乗り物のオブジェクトを設定します。

### 乗り物のオブジェクトの中に椅子を置く場合

1. InsideUserMovingFloorVRCChair.prefabを乗り物に置いて下さい。
2. UserMovingFloorのInside Chairsに上記で置いた中のVRCChair3オブジェクト（InsideVRCStationCompanionが付いている）を設定して下さい。

- 既知の問題: 椅子に先に座っている人がいる場合もInteractできてしまいます。

## プルリクエスト歓迎！

雑に投げて下さい。反応がなければTwitterでつついて下さい。是非よりよい乗車システムを作ってゆきましょう。

## ライセンス

[Zlib License](LICENSE)

Zlibライセンスなので普通に使う場合は表記も全く必要ないんですが、VRChatのワールドに使う場合は書いておくと良い乗り物ワールドが増える気がするので書いておくのオススメ。
