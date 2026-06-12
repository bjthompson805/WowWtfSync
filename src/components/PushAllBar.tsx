import { ArrowUp } from 'lucide-react';
import { ADDONS, type Addon } from '../types';

interface Props {
  onPushAll: (addon: Addon) => void;
}

export default function PushAllBar({ onPushAll }: Props) {
  return (
    <div className="push-all-bar">
      <span className="push-all-label">Push All:</span>
      {ADDONS.map(addon => (
        <button
          key={addon}
          className="btn btn-push-all"
          onClick={() => onPushAll(addon)}
          title={`Push ${addon} for all enabled characters`}
        >
          <ArrowUp size={13} />
          <span>{addon}</span>
        </button>
      ))}
    </div>
  );
}
