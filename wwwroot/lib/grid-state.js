/**
 * GridState - Manages the state of the game grid
 */
class GridState {
  constructor(size = 5) {
    this.size = size;
    this.currentRow = 0;
    this.currentCol = 0;
    this.droppables = new Set(); // Store droppable positions as "row,col" strings
  }

  /**
   * Add a droppable at specific position
   */
  addDroppable(row, col) {
    this.droppables.add(`${row},${col}`);
  }

  /**
   * Check if position has a droppable
   */
  hasDroppable(row, col) {
    return this.droppables.has(`${row},${col}`);
  }

  /**
   * Remove droppable at position (when picked up)
   */
  removeDroppable(row, col) {
    return this.droppables.delete(`${row},${col}`);
  }

  /**
   * Get all droppable positions
   */
  getDroppables() {
    return Array.from(this.droppables).map(pos => {
      const [row, col] = pos.split(',').map(Number);
      return { row, col };
    });
  }

  /**
   * Get current position as object
   */
  getPosition() {
    return { row: this.currentRow, col: this.currentCol };
  }

  /**
   * Set position (with boundary checking)
   */
  setPosition(row, col) {
    this.currentRow = this.clampRow(row);
    this.currentCol = this.clampCol(col);
    return this.getPosition();
  }

  /**
   * Clamp row value within grid bounds
   */
  clampRow(row) {
    return Math.max(0, Math.min(this.size - 1, row));
  }

  /**
   * Clamp column value within grid bounds
   */
  clampCol(col) {
    return Math.max(0, Math.min(this.size - 1, col));
  }

  /**
   * Move in a specific direction
   * Returns true if position changed, false otherwise
   */
  move(direction) {
    const oldRow = this.currentRow;
    const oldCol = this.currentCol;

    switch(direction) {
      case 'left':
        this.currentCol = this.clampCol(this.currentCol - 1);
        break;
      case 'right':
        this.currentCol = this.clampCol(this.currentCol + 1);
        break;
      case 'up':
        this.currentRow = this.clampRow(this.currentRow - 1);
        break;
      case 'down':
        this.currentRow = this.clampRow(this.currentRow + 1);
        break;
      default:
        return false;
    }

    return oldRow !== this.currentRow || oldCol !== this.currentCol;
  }

  /**
   * Reset to starting position
   */
  reset() {
    this.currentRow = 0;
    this.currentCol = 0;
  }
}
