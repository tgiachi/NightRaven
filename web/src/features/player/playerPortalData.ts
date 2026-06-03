export type PlayerCharacter = {
  name: string
  title: string
  shard: string
  lastLogin: string
  level: number
  crest: 'lion' | 'star' | 'stag'
}

export type PatchNote = {
  date: string
  title: string
  summary: string
}

export const playerCharacters: PlayerCharacter[] = [
  {
    name: 'Arthorius',
    title: 'Grandmaster Warrior',
    shard: 'Britannia',
    lastLogin: 'Today',
    level: 120,
    crest: 'lion',
  },
  {
    name: 'Liriana',
    title: 'Elder Mage',
    shard: 'Trammel',
    lastLogin: 'Yesterday',
    level: 110,
    crest: 'star',
  },
  {
    name: 'Khaldun',
    title: 'Master Ranger',
    shard: 'Ilshenar',
    lastLogin: '3 days ago',
    level: 106,
    crest: 'stag',
  },
]

export const patchNotes: PatchNote[] = [
  {
    date: 'May 28, 2025',
    title: 'Spring Season: Siege Warfare Begins',
    summary: 'Factions prepare your armies. Castle sieges are now live.',
  },
  {
    date: 'May 21, 2025',
    title: 'Client Patch 1.9.7',
    summary: 'Includes balance updates, bug fixes, and stability improvements.',
  },
  {
    date: 'May 14, 2025',
    title: 'Developer Journal #15',
    summary: 'Behind the scenes of our upcoming systems and content.',
  },
]
