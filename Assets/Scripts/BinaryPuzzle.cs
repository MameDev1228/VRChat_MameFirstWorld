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
/// プレイヤーは端末に表示された二進数を10進数に変換し、正しい数字を入力することで
/// ドアが開く仕組みになっています。
/// 
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BinaryPuzzle : UdonSharpBehaviour
{
    [Header("ドアオブジェクト")]
    public GameObject doorObject; // 開閉対象のドア
    private Animator doorAnimator; // ドアアニメーター

    [Header("端末オブジェクト")]
    public GameObject terminalDisplay; // 端末表示用のテキストオブジェクト（TextMeshProなど推奨）

    private string binaryCode; // 現在の二進数コード
    private int decimalAnswer; // 二進数を10進数に変換した正解

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

        // 最初の二進数コードを生成（8桁）
        binaryCode = GenerateBinaryCode(8);
        decimalAnswer = BinaryToDecimal(binaryCode); // 自前関数で変換

        // -------------------------
        // 二進数→10進数変換関数
        int BinaryToDecimal(string binary)
        {
            int result = 0;
            for (int i = 0; i < binary.Length; i++)
            {
                // 左から桁を読む
                if (binary[i] == '1')
                {
                    // 2^(桁の位置)を足す
                    result += 1 << (binary.Length - i - 1);
                }
            }
            return result;
        }

        // 端末にコードを表示
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
            if (inputNumber == decimalAnswer)
            {
                // 正解
                OpenDoor();
                ShowMessage("正解です！ドアが開きました！");
            }
            else
            {
                // 不正解
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
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open"); // AnimatorにOpenトリガーを送信
        }
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
    /// 4桁のランダム二進数を生成
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
