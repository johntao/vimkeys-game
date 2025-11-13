/**
 * VimNavigation - Handles vim-style keyboard navigation
 */
class VimNavigation {
  constructor(gridState, onMove) {
    this.gridState = gridState;
    this.onMove = onMove; // Callback function when position changes
    this.keyMap = {
      'h': 'left',
      'j': 'down',
      'k': 'up',
      'l': 'right'
    };
    this.boundHandler = this.handleKeyDown.bind(this);
  }

  /**
   * Handle keyboard events
   */
  handleKeyDown(e) {
    const key = e.key.toLowerCase();
    const direction = this.keyMap[key];

    if (!direction) {
      return; // Not a vim navigation key
    }

    const moved = this.gridState.move(direction);

    if (moved && this.onMove) {
      this.onMove(this.gridState.getPosition(), direction);
      e.preventDefault();
    }
  }

  /**
   * Start listening for keyboard events
   */
  enable() {
    document.addEventListener('keydown', this.boundHandler);
  }

  /**
   * Stop listening for keyboard events
   */
  disable() {
    document.removeEventListener('keydown', this.boundHandler);
  }

  /**
   * Add custom key mapping
   */
  addKey(key, direction) {
    this.keyMap[key.toLowerCase()] = direction;
  }

  /**
   * Remove key mapping
   */
  removeKey(key) {
    delete this.keyMap[key.toLowerCase()];
  }
}
