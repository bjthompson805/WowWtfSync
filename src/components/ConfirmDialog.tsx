interface Props {
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmDialog({ message, onConfirm, onCancel }: Props) {
  return (
    <div className="dialog-backdrop">
      <div className="dialog confirm-dialog">
        <p className="dialog-message">{message}</p>
        <div className="dialog-footer">
          <button className="btn btn-danger" onClick={onConfirm}>Yes</button>
          <button className="btn" onClick={onCancel}>No</button>
        </div>
      </div>
    </div>
  );
}
