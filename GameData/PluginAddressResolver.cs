namespace XIVComboVX.GameData;

using System;
using System.Text;

using Dalamud.Game;
using Dalamud.Logging;

internal class PluginAddressResolver: BaseAddressResolver {
	private const string addrFmtSpec = "X16";

	public Exception? LoadFailReason { get; private set; }
	public bool LoadSuccessful => this.LoadFailReason is null;

	public IntPtr ComboTimer { get; private set; } = IntPtr.Zero;
	public string ComboTimerAddr => this.ComboTimer.ToInt64().ToString(addrFmtSpec);

	public IntPtr LastComboMove => this.ComboTimer + 0x4;
	public string LastComboMoveAddr => this.LastComboMove.ToInt64().ToString(addrFmtSpec);

	public IntPtr GetAdjustedActionId { get; private set; } = IntPtr.Zero;
	public string GetAdjustedActionIdAddr => this.GetAdjustedActionId.ToInt64().ToString(addrFmtSpec);

	public IntPtr IsActionIdReplaceable { get; private set; } = IntPtr.Zero;
	public string IsActionIdReplaceableAddr => this.IsActionIdReplaceable.ToInt64().ToString(addrFmtSpec);


	protected override void Setup64Bit(SigScanner scanner) {
		try {
			PluginLog.Debug("Scanning for ComboTimer signature");
			this.ComboTimer = scanner.GetStaticAddressFromSig("F3 0F 11 05 ?? ?? ?? ?? F3 0F 10 45 ?? E8");

			PluginLog.Debug("Scanning for GetAdjustedActionId signature");
			this.GetAdjustedActionId = scanner.ScanText("E8 ?? ?? ?? ?? 8B F8 3B DF");  // Client::Game::ActionManager.GetAdjustedActionId

			PluginLog.Debug("Scanning for IsActionIdReplaceable signature");
			this.IsActionIdReplaceable = scanner.ScanText("81 F9 ?? ?? ?? ?? 7F 35");
		}
		catch (Exception ex) {
			this.LoadFailReason = ex;
			StringBuilder msg = new();
			msg.AppendLine("Address scanning failed, plugin cannot load.");
			msg.AppendLine("Please present this error message to the developer.");
			msg.AppendLine();
			msg.Append("Signature scan failed for ");
			if (this.ComboTimer == IntPtr.Zero)
				msg.Append("ComboTimer");
			else if (this.GetAdjustedActionId == IntPtr.Zero)
				msg.Append("GetAdjustedActionId");
			else if (this.IsActionIdReplaceable == IntPtr.Zero)
				msg.Append("IsActionIdReplaceable");
			msg.AppendLine(":");
			msg.Append(ex.ToString());
			PluginLog.Fatal(msg.ToString());
			return;
		}

		Service.Logger.debug("Address resolution successful");

		PluginLog.Verbose($"GetAdjustedActionId 0x{this.GetAdjustedActionIdAddr}");
		PluginLog.Verbose($"IsIconReplaceable   0x{this.IsActionIdReplaceableAddr}");
		PluginLog.Verbose($"ComboTimer          0x{this.ComboTimerAddr}");
		PluginLog.Verbose($"LastComboMove       0x{this.LastComboMoveAddr}");
	}
}
