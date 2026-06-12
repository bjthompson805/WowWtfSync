import { ArrowUp } from 'lucide-react';
import { ADDONS, type Addon } from '../types';

interface Props {
  onPushAll: (addon: Addon | 'all') => void;
}

export default function PushAllBar({ onPushAll }: Props) {
  return (
    <div className="push-all-bar">
      <span className="push-all-label">Push All:</span>
      <button
        className="btn btn-push-all btn-push-all-all"
        onClick={() => onPushAll('all')}
        title="Push all addons for all enabled characters"
      >
        <ArrowUp size={13} />
        <span>ALL</span>
      </button>
      <span className="push-all-separator" />
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
