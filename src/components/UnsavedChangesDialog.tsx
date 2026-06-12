interface Props {
  title: string;
  primaryLabel: string;
  secondaryLabel: string;
  onPrimary: () => void;
  onSecondary: () => void;
  onCancel: () => void;
}

export default function UnsavedChangesDialog({
  title,
  primaryLabel,
  secondaryLabel,
  onPrimary,
  onSecondary,
  onCancel,
}: Props) {
  return (
    <div className="dialog-backdrop">
      <div className="dialog unsaved-dialog">
        <p className="dialog-message">{title}</p>
        <div className="dialog-footer unsaved-dialog-footer">
          <button className="btn" onClick={onCancel}>Cancel</button>
          <div className="unsaved-dialog-actions">
            <button className="btn" onClick={onSecondary}>{secondaryLabel}</button>
            <button className="btn btn-primary" onClick={onPrimary}>{primaryLabel}</button>
          </div>
        </div>
      </div>
    </div>
  );
}
