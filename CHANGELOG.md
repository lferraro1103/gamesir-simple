# Changelog

## Unreleased

- Fix a startup crash when a saved response curve (or any stick setting) was above 100.
- Fix language switching resetting edited stick settings to the values from app startup.
- Show D-pad bindings with their direction arrow instead of the raw hat bit number.
- Open the window with a single left click on the tray icon (double-click still works).
- Read the embedded mapping correctly even if the file is checked out with CRLF line endings.
- Draw the live stick pointer on top of L3/R3 and on the same scale as the dead zone, so small movements are visible.
- Grey out the Xbox button while no controller is connected, and show it in red while emulation is active.
- Place stick setting fields next to their labels and align the checkboxes and buttons on that tab.
- Remove the misleading pre-selected first row in the mapping table and give it clearer headers.
- Show "—" in the live readout when no button is pressed.
- Refresh the README screenshots.
- Add offline regression checks for these fixes.

## v1.2.0

- Add an instant, persistent Spanish / English / Simplified Chinese language selector.
- Translate controls, status, mapping names, tooltips, errors and tray commands.
- Keep the app and emulation running in the notification area when X is clicked.
- Reopen from the tray icon, toggle Xbox from its menu, or exit completely.
- Allow Windows shutdown and explicit exit to release the virtual controller.
- Remove the external-emulator banner and process-based activation block.
- Keep the main screen focused on GameSir Simple controls and saved settings.
- Embed the X5 Lite mapping in the executable, eliminating the external mapping-file requirement.
- Clear inputs and stop output immediately when the physical controller disconnects.
- Validate saved settings and preserve valid bindings when individual entries are malformed.
- Save settings through a temporary file before replacement.
- Reject center calibration while a stick is pushed far from rest.
- Release the polling timer and input engine when the form is disposed.
- Expand offline checks to all 25 assignment events, input shaping, settings, embedded mapping and three rendered app screens.
- Update all three language READMEs.

## v1.1.0

- First GitHub release of GameSir Simple for the GameSir X5 Lite.
- Add a custom application and window icon.
- Add an interactive controller diagram with 25 clickable remapping targets.
- Highlight pressed inputs in turquoise and the recording target in orange.
- Restore individual automatic mappings from the diagram's context menu.
- Preserve button/axis mapping, stick dead zones, response curves, inversion, center calibration and saved settings.
- Include English, Spanish and Simplified Chinese READMEs.
- Verify offline hit-testing at three sizes, idle/active rendering and the embedded icon.

## Earlier local version

- USB detection of the correct X5 Lite interface among its three exposed devices.
- Xbox 360 emulation through an installed ViGEmBus driver.
- Physical detection and virtual report verified through Windows XInput.
