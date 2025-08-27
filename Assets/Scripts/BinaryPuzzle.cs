using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// BinaryPuzzle.cs
/// 
/// 制作者: Mame_Dev
/// 
/// VRChat用ファーストステージの二進数→十進数パズルスクリプト
/// プレイヤーは端末に表示された二進数を10進数に変換し、正しい数字を入力すると
/// ドアが開く協力プレイ向けマルチ対応版
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class BinaryPuzzle : UdonSharpBehaviour
{
    [Header("ドアオブジェクト")]
    public GameObject doorObject; // 開閉対象のドア
    private Animator doorAnimator; // ドアアニメーター

    [Header("端末オブジェクト")]
    public GameObject terminalDisplay; // 端末表示用のテキストオブジェクト（TextMeshProなど推奨）

    [UdonSynced] private string binaryCode;   // 同期される二進数コード
    [UdonSynced] private int decimalAnswer;   // 同期される正解値
    [UdonSynced] private bool doorOpened;     // ドアの開閉状態を同期

    void Start()
    {
        // ドアAnimator取得
        if (doorObject != null)
        {
            doorAnimator = doorObject.GetComponent<Animator>();
            if (doorAnimator == null)
            {
                Debug.LogWarning("DoorオブジェクトにAnimatorがアタッチされていません");
            }
        }

        // マスタークライアントだけがコード生成
        if (Networking.IsMaster)
        {
            binaryCode = GenerateBinaryCode(8);       // 8桁の二進数
            decimalAnswer = BinaryToDecimal(binaryCode); // 自前関数で変換
            doorOpened = false;
            RequestSerialization(); // 全員に同期
        }

        // 端末表示を初期化
        UpdateTerminalDisplay(binaryCode);
    }

    /// <summary>
    /// プレイヤー入力をチェックする
    /// </summary>
    /// <param name="playerInput">プレイヤーが端末に入力した文字列</param>
    public void CheckInput(string playerInput)
    {
        if (int.TryParse(playerInput, out int inputNumber))
        {
            if (inputNumber == decimalAnswer && !doorOpened)
            {
                // 正解したプレイヤーがドアを開く
                OpenDoor();
                ShowMessage("正解です！ドアが開きました！");
            }
            else if (!doorOpened)
            {
                ShowMessage("違います…もう一度計算してみてください！");
            }
        }
        else
        {
            // 数字以外入力時
            ShowMessage("数字を入力してください！");
        }
    }

    /// <summary>
    /// ドアを開く
    /// </summary>
    private void OpenDoor()
    {
        if (!doorOpened)
        {
            doorOpened = true;
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Open");
            }
            RequestSerialization(); // 全員に同期
        }
    }

    /// <summary>
    /// 他プレイヤーから同期情報が届いたとき
    /// </summary>
    public override void OnDeserialization()
    {
        // ドア状態を反映
        if (doorOpened && doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }

        // 端末表示を同期
        UpdateTerminalDisplay(binaryCode);
    }

    /// <summary>
    /// 端末表示を更新
    /// </summary>
    /// <param name="code">表示する二進数文字列</param>
    private void UpdateTerminalDisplay(string code)
    {
        if (terminalDisplay != null)
        {
            var textMesh = terminalDisplay.GetComponent<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = "CODE: " + code;
            }
        }
    }

    /// <summary>
    /// 指定桁数のランダム二進数を生成
    /// </summary>
    /// <param name="length">桁数</param>
    /// <returns>二進数文字列</returns>
    private string GenerateBinaryCode(int length)
    {
        string result = "";
        for (int i = 0; i < length; i++)
        {
            result += Random.Range(0, 2).ToString();
        }
        return result;
    }

    /// <summary>
    /// 二進数文字列を10進数に変換
    /// </summary>
    /// <param name="binary">二進数文字列</param>
    /// <returns>10進数値</returns>
    private int BinaryToDecimal(string binary)
    {
        int result = 0;
        for (int i = 0; i < binary.Length; i++)
        {
            if (binary[i] == '1')
            {
                result += 1 << (binary.Length - i - 1);
            }
        }
        return result;
    }

    /// <summary>
    /// 端末にメッセージを表示（デバッグ/演出用）
    /// </summary>
    /// <param name="message">表示メッセージ</param>
    private void ShowMessage(string message)
    {
        if (terminalDisplay != null)
        {
            var textMesh = terminalDisplay.GetComponent<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = message;
            }
        }
    }
}
