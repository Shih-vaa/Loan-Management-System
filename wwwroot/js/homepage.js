
    document.addEventListener('DOMContentLoaded', () => {
        setupCounterAnimation();
        setupSmoothScrolling();
        loadNotifications(); // already defined earlier

        // Refresh notifications every 30s
        setInterval(loadNotifications, 30000);

        // Reload when dropdown opened
        document.getElementById('notificationDropdown')?.addEventListener('shown.bs.dropdown', loadNotifications);
    });

    // ✅ Stat Counter Animation
    function setupCounterAnimation() {
        const statsSection = document.querySelector('.stats-section');
        if (!statsSection) return;

        const observer = new IntersectionObserver((entries) => {
            if (entries[0].isIntersecting) {
                animateCounters();
                observer.unobserve(statsSection);
            }
        }, { threshold: 0.5 });

        observer.observe(statsSection);
    }

    function animateCounters() {
        const counters = document.querySelectorAll('.stat-number');

        counters.forEach(counter => {
            const target = parseInt(counter.getAttribute('data-target')) || 0;
            let current = 0;
            const speed = 20;

            const update = () => {
                const increment = Math.ceil(target / 100);
                current += increment;
                if (current >= target) {
                    counter.innerText = target.toLocaleString();
                    clearInterval(timer);
                } else {
                    counter.innerText = current.toLocaleString();
                }
            };

            const timer = setInterval(update, speed);
        });
    }

    // ✅ Smooth Scrolling for Anchor Links
    function setupSmoothScrolling() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                if (href && href !== "#") {
                    e.preventDefault();
                    const targetElement = document.querySelector(href);
                    if (targetElement) {
                        targetElement.scrollIntoView({ behavior: 'smooth' });
                    }
                }
            });
        });
    }

