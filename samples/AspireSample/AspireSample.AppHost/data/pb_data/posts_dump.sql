-- sqlite3 .\data.db ".output posts_dump.sql" ".dump posts"
-- sqlite3 .\data.db ".read posts_dump.sql"
PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE `posts` (`author` TEXT DEFAULT '' NOT NULL, `content` TEXT DEFAULT '' NOT NULL, `created` TEXT DEFAULT '' NOT NULL, `id` TEXT PRIMARY KEY DEFAULT ('r'||lower(hex(randomblob(7)))) NOT NULL, `title` TEXT DEFAULT '' NOT NULL, `updated` TEXT DEFAULT '' NOT NULL, "is_published" BOOLEAN DEFAULT FALSE NOT NULL, "slug" TEXT DEFAULT '' NOT NULL);
INSERT INTO posts VALUES('Alice','<p>This is the content of the first post.</p>','2025-12-21 15:28:00.564Z','lxe9zgmyl4vxu48','First Post','2025-12-21 17:16:40.912Z',1,'first-post');
INSERT INTO posts VALUES('Bob','<p>Another interesting post for the blog.</p>','2025-12-21 15:29:01.943Z','43jqe8ij7oy9afy','Second Post','2025-12-21 17:16:56.220Z',1,'second-post');
INSERT INTO posts VALUES('Charlie','<p>Blogging is fun with PocketBase!</p>','2025-12-21 15:29:34.839Z','j4ac9spkucdtcb9','Third Post','2025-12-21 17:16:16.158Z',1,'third-post');
INSERT INTO posts VALUES('Dana','<p>Let&rsquo;s add more dummy data.</p>','2025-12-21 15:29:49.704Z','yul1x8ua5qj5v9v','Fourth Post','2025-12-21 17:15:48.974Z',1,'fourth-post');
INSERT INTO posts VALUES('Eve','<p>Halfway through our dummy posts.</p>','2025-12-21 15:30:03.028Z','trq1wxw7ch2aj3r','Fifth Post','2025-12-21 17:15:57.230Z',0,'fifth-post');
INSERT INTO posts VALUES('Frank','<p>This post is just for testing.</p>','2025-12-21 15:30:14.334Z','1zmto1t5k4ntnk9','Sixth Post','2025-12-21 17:15:14.824Z',1,'sixth-post');
INSERT INTO posts VALUES('Grace','<p>Seventh time&rsquo;s the charm.</p>','2025-12-21 15:30:30.494Z','cwpfp05lpnopbq6','Seventh Post','2025-12-21 17:15:00.808Z',1,'seventh-post');
INSERT INTO posts VALUES('Heidi','<p>Almost done with our sample data.</p>','2025-12-21 15:30:43.077Z','7sfcqks933gfe1z','Eighth Post','2025-12-21 17:17:08.591Z',0,'eighth-post');
INSERT INTO posts VALUES('Ivan','<p>Just one more after this!</p>','2025-12-21 15:30:56.036Z','b4su072kshh5xet','Ninth Post','2025-12-21 17:14:29.833Z',1,'ninth-post');
INSERT INTO posts VALUES('Judy','<p>This is the last dummy post.</p>','2025-12-21 15:31:13.413Z','ixodttnw2w3jouz','Tenth Post','2025-12-21 17:14:12.739Z',1,'tenth-post');
COMMIT;
